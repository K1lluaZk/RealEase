using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RealEase.Application.Dtos.Payment;
using RealEase.Application.Services;
using RealEase.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly ContractService _contractService;

        public PaymentsController(PaymentService paymentService, ContractService contractService)
        {
            _paymentService = paymentService;
            _contractService = contractService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PaymentDto>>> GetAll()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            if (User.IsInRole(UserRoles.Client) &&
                Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId))
                payments = payments.Where(payment => payment.TenantId == clientId).ToList();
            else if (User.IsInRole(UserRoles.Agent) &&
                     Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var agentId))
            {
                var assignedContracts = await _contractService.GetAllContractsAsync();
                var assignedContractIds = assignedContracts
                    .Where(contract => contract.AgentId == agentId)
                    .Select(contract => contract.Id)
                    .ToHashSet();
                payments = payments
                    .Where(payment => payment.ContractId.HasValue && assignedContractIds.Contains(payment.ContractId.Value))
                    .ToList();
            }

            return Ok(payments);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<PaymentDto>> GetById(long id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound($"Pago con ID {id} no encontrado.");
            if (User.IsInRole(UserRoles.Client) &&
                (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clientId) ||
                 payment.TenantId != clientId))
                return Forbid();
            if (User.IsInRole(UserRoles.Agent))
            {
                var contract = payment.ContractId.HasValue
                    ? await _contractService.GetContractByIdAsync(payment.ContractId.Value)
                    : null;
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var agentId) ||
                    contract?.AgentId != agentId)
                    return Forbid();
            }

            return Ok(payment);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<long>> Create([FromBody] PaymentDto dto)
        {
            var paymentId = await _paymentService.AddPaymentAsync(dto);
            if (paymentId == 0) return BadRequest("No se pudo registrar el pago.");

            return CreatedAtAction(nameof(GetById), new { id = paymentId }, paymentId);
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Update([FromBody] PaymentDto dto)
        {
            var result = await _paymentService.UpdatePaymentAsync(dto);
            if (!result) return NotFound("Pago no encontrado para actualizar.");
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _paymentService.DeletePaymentAsync(id);
            if (!result) return NotFound("Pago no encontrado.");
            return NoContent();
        }
    }
}
