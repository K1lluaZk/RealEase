using Microsoft.AspNetCore.Mvc;
using RealEase.Application.Dtos.Visit;
using RealEase.Application.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitsController : ControllerBase
    {
        private readonly VisitService _visitService;

        public VisitsController(VisitService visitService)
        {
            _visitService = visitService;
        }

        [HttpGet]
        public async Task<ActionResult<List<VisitDto>>> GetAll()
        {
            var visits = await _visitService.GetAllVisitsAsync();
            return Ok(visits);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<VisitDto>> GetById(long id)
        {
            var visit = await _visitService.GetVisitByIdAsync(id);
            if (visit == null) return NotFound($"Visita con ID {id} no encontrada.");
            return Ok(visit);
        }

        [HttpPost]
        public async Task<ActionResult<long>> Create([FromBody] VisitDto dto)
        {
            var visitId = await _visitService.AddVisitAsync(dto);
            if (visitId == 0) return BadRequest("No se pudo agendar la visita.");

            return CreatedAtAction(nameof(GetById), new { id = visitId }, visitId);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] VisitDto dto)
        {
            var result = await _visitService.UpdateVisitAsync(dto);
            if (!result) return NotFound("Visita no encontrada para actualizar.");
            return NoContent();
        }

        [HttpDelete("{id:long}")]
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
            return Ok(visits);
        }
    }
}
