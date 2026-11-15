using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RealEase.Application.Dtos.Property;
using RealEase.Application.Services;
using RealEase.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PropertiesController : ControllerBase
    {
        private readonly PropertyService _propertyService;

        public PropertiesController(PropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<PropertyDto>>> GetAll()
        {
            var properties = await _propertyService.GetAllPropertiesAsync();
            return Ok(properties);
        }

        [HttpGet("{id:long}")]
        [AllowAnonymous]
        public async Task<ActionResult<PropertyDto>> GetById(long id)
        {
            var property = await _propertyService.GetPropertyByIdAsync(id);
            if (property == null) return NotFound($"Propiedad con ID {id} no encontrada.");
            return Ok(property);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<long>> Create([FromBody] PropertyDto dto)
        {
            var propertyId = await _propertyService.AddPropertyAsync(dto);
            if (propertyId == 0) return BadRequest("No se pudo registrar la propiedad.");

            return CreatedAtAction(nameof(GetById), new { id = propertyId }, propertyId);
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Update([FromBody] PropertyDto dto)
        {
            var result = await _propertyService.UpdatePropertyAsync(dto);
            if (!result) return NotFound("Propiedad no encontrada para actualizar.");
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _propertyService.DeletePropertyAsync(id);
            if (!result) return NotFound("Propiedad no encontrada.");
            return NoContent();
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<ActionResult<List<PropertyDto>>> GetFiltered(
            [FromQuery] string? address,
            [FromQuery] string? type,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? status)
        {
            var properties = await _propertyService.GetFilteredPropertiesAsync(address, type, minPrice, maxPrice, status);
            return Ok(properties);
        }
    }
}
