using Microsoft.AspNetCore.Mvc;
using RealEase.Application.Dtos.Contract;
using RealEase.Application.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.WebAPI.Controllers
{
    [ApiController]
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
            return Ok(contracts);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ContractDto>> GetById(long id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            if (contract == null) return NotFound($"Contrato con ID {id} no encontrado.");
            return Ok(contract);
        }

        [HttpPost]
        public async Task<ActionResult<long>> Create([FromBody] ContractDto dto)
        {
            var contractId = await _contractService.AddContractAsync(dto);
            if (contractId == 0) return BadRequest("No se pudo registrar el contrato.");

            return CreatedAtAction(nameof(GetById), new { id = contractId }, contractId);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ContractDto dto)
        {
            var result = await _contractService.UpdateContractAsync(dto);
            if (!result) return NotFound("Contrato no encontrado para actualizar.");
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _contractService.DeleteContractAsync(id);
            if (!result) return NotFound("Contrato no encontrado.");
            return NoContent();
        }
    }
}
