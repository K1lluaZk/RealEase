using Microsoft.AspNetCore.Mvc;
using RealEase.Application.Dtos.Payment;
using RealEase.Application.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentsController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PaymentDto>>> GetAll()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<PaymentDto>> GetById(long id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound($"Pago con ID {id} no encontrado.");
            return Ok(payment);
        }

        [HttpPost]
        public async Task<ActionResult<long>> Create([FromBody] PaymentDto dto)
        {
            var paymentId = await _paymentService.AddPaymentAsync(dto);
            if (paymentId == 0) return BadRequest("No se pudo registrar el pago.");

            return CreatedAtAction(nameof(GetById), new { id = paymentId }, paymentId);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] PaymentDto dto)
        {
            var result = await _paymentService.UpdatePaymentAsync(dto);
            if (!result) return NotFound("Pago no encontrado para actualizar.");
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _paymentService.DeletePaymentAsync(id);
            if (!result) return NotFound("Pago no encontrado.");
            return NoContent();
        }
    }
}
