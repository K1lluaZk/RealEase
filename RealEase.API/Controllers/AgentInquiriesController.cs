using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEase.Application.Dtos.AgentInquiry;
using RealEase.Application.Services;

namespace RealEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AgentInquiriesController(AgentInquiryService inquiryService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<AgentInquiryResponseDto>> Create(
        [FromBody] AgentInquiryDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await inquiryService.CreateAsync(dto, cancellationToken);
            return response is null
                ? NotFound("No hay un agente activo asignado a esa ubicación.")
                : Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
