using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RealEase.Application.Dtos.Visit;
using RealEase.Application.Services;
using RealEase.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class VisitsController : ControllerBase
    {
        private readonly VisitService _visitService;
        private readonly PropertyService _propertyService;

        public VisitsController(VisitService visitService, PropertyService propertyService)
        {
            _visitService = visitService;
            _propertyService = propertyService;
        }

        [HttpGet]
        public async Task<ActionResult<List<VisitDto>>> GetAll()
        {
            var visits = await _visitService.GetAllVisitsAsync();
            if (User.IsInRole(UserRoles.Client) &&
                Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId))
                visits = visits.Where(visit => visit.UserId == clientId).ToList();
            else if (User.IsInRole(UserRoles.Agent))
            {
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var agentId))
                    return Forbid();

                var properties = await _propertyService.GetAllPropertiesAsync();
                var managedPropertyIds = properties
                    .Where(property => property.OwnerId == agentId)
                    .Select(property => property.Id)
                    .ToHashSet();
                visits = visits.Where(visit => managedPropertyIds.Contains(visit.PropertyId)).ToList();
            }

            return Ok(visits);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<VisitDto>> GetById(long id)
        {
            var visit = await _visitService.GetVisitByIdAsync(id);
            if (visit == null) return NotFound($"Visita con ID {id} no encontrada.");
            if (User.IsInRole(UserRoles.Client) && !IsCurrentClient(visit.UserId))
                return Forbid();
            if (User.IsInRole(UserRoles.Agent) && !await IsManagedPropertyAsync(visit.PropertyId))
                return Forbid();
            return Ok(visit);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Client)]
        public async Task<ActionResult<long>> Create([FromBody] VisitDto dto)
        {
            var property = await _propertyService.GetPropertyByIdAsync(dto.PropertyId);
            if (property is null)
                return NotFound("Propiedad no encontrada.");
            if (!string.Equals(property.ListingType, "sale", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Solo se pueden agendar visitas para propiedades en venta.");

            if (User.IsInRole(UserRoles.Client))
            {
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId))
                    return Forbid();

                dto.UserId = clientId;
                dto.Status = "pending";
            }
            var visitId = await _visitService.AddVisitAsync(dto);
            if (visitId == 0) return BadRequest("No se pudo agendar la visita.");

            return CreatedAtAction(nameof(GetById), new { id = visitId }, visitId);
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Agent)]
        public async Task<IActionResult> Update([FromBody] VisitDto dto)
        {
            if (User.IsInRole(UserRoles.Agent))
            {
                var existingVisit = await _visitService.GetVisitByIdAsync(dto.Id);
                if (existingVisit is null)
                    return NotFound("Visita no encontrada para actualizar.");
                if (!await IsManagedPropertyAsync(existingVisit.PropertyId))
                    return Forbid();

                dto.UserId = existingVisit.UserId;
                dto.PropertyId = existingVisit.PropertyId;
            }

            var result = await _visitService.UpdateVisitAsync(dto);
            if (!result) return NotFound("Visita no encontrada para actualizar.");
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _visitService.DeleteVisitAsync(id);
            if (!result) return NotFound("Visita no encontrada.");
            return NoContent();
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<VisitDto>>> GetFiltered(
            [FromQuery] DateTime? visitDate,
            [FromQuery] long? propertyId,
            [FromQuery] Guid? clientId,
            [FromQuery] string? status)
        {
            var visits = await _visitService.GetFilteredVisitsAsync(visitDate, propertyId, clientId, status);
            if (User.IsInRole(UserRoles.Client) &&
                Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentClientId))
                visits = visits.Where(visit => visit.UserId == currentClientId).ToList();
            else if (User.IsInRole(UserRoles.Agent))
            {
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var agentId))
                    return Forbid();

                var properties = await _propertyService.GetAllPropertiesAsync();
                var managedPropertyIds = properties
                    .Where(property => property.OwnerId == agentId)
                    .Select(property => property.Id)
                    .ToHashSet();
                visits = visits.Where(visit => managedPropertyIds.Contains(visit.PropertyId)).ToList();
            }

            return Ok(visits);
        }

        private bool IsCurrentClient(Guid userId) =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentClientId) &&
            currentClientId == userId;

        private async Task<bool> IsManagedPropertyAsync(long propertyId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var agentId))
                return false;

            var property = await _propertyService.GetPropertyByIdAsync(propertyId);
            return property?.OwnerId == agentId;
        }
    }
}
