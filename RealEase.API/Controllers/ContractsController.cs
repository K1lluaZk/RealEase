using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RealEase.Application.Dtos.Contract;
using RealEase.Application.Services;
using RealEase.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly ContractService _contractService;

        public ContractsController(ContractService contractService)
        {
            _contractService = contractService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ContractDto>>> GetAll()
        {
            var contracts = await _contractService.GetAllContractsAsync();
            if (User.IsInRole(UserRoles.Client) &&
                Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId))
                contracts = contracts.Where(contract => contract.ClientId == clientId).ToList();
            else if (User.IsInRole(UserRoles.Agent) &&
                     Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var agentId))
                contracts = contracts.Where(contract => contract.AgentId == agentId).ToList();

            return Ok(contracts);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ContractDto>> GetById(long id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            if (contract == null) return NotFound($"Contrato con ID {id} no encontrado.");
            if (User.IsInRole(UserRoles.Client) &&
                (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId) ||
                 contract.ClientId != clientId))
                return Forbid();
            if (User.IsInRole(UserRoles.Agent) &&
                (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var agentId) ||
                 contract.AgentId != agentId))
                return Forbid();

            return Ok(contract);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Agent)]
        public async Task<ActionResult<long>> Create([FromBody] ContractDto dto)
        {
            if (User.IsInRole(UserRoles.Agent))
            {
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var agentId))
                    return Forbid();

                dto.AgentId = agentId;
            }

            var contractId = await _contractService.AddContractAsync(dto);
            if (contractId == 0) return BadRequest("No se pudo registrar el contrato.");

            return CreatedAtAction(nameof(GetById), new { id = contractId }, contractId);
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Agent)]
        public async Task<IActionResult> Update([FromBody] ContractDto dto)
        {
            if (User.IsInRole(UserRoles.Agent))
            {
                var existingContract = await _contractService.GetContractByIdAsync(dto.Id);
                if (existingContract is null)
                    return NotFound("Contrato no encontrado para actualizar.");

                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var agentId) ||
                    existingContract.AgentId != agentId)
                    return Forbid();

                if (!string.Equals(dto.Status, existingContract.Status, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("La aprobación del contrato requiere verificar el pago y aún no está habilitada.");

                dto.ClientId = existingContract.ClientId;
                dto.PropertyId = existingContract.PropertyId;
                dto.AgentId = agentId;
            }

            var result = await _contractService.UpdateContractAsync(dto);
            if (!result) return NotFound("Contrato no encontrado para actualizar.");
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _contractService.DeleteContractAsync(id);
            if (!result) return NotFound("Contrato no encontrado.");
            return NoContent();
        }
    }
}
