using Microsoft.AspNetCore.Mvc;
using RealEase.Web.Services;
using RealEase.Application.Dtos.Payment;

namespace RealEase.Web.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly RealEaseApiClient _api;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(RealEaseApiClient api, ILogger<PaymentsController> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                return View(await _api.GetPaymentsAsync(cancellationToken));
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                _logger.LogWarning(exception, "No se pudieron cargar pagos desde la API.");
                ViewData["ApiError"] = "No se pudieron cargar los datos. Comprueba que RealEase.API esté activa y que su conexión a la base de datos funcione.";
                return View(new List<PaymentDto>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFromApi(long id, CancellationToken cancellationToken)
        {
            try
            {
                await _api.DeleteAsync($"api/Payments/{id}", cancellationToken);
                TempData["Success"] = "Pago eliminado.";
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo eliminar el pago {PaymentId} mediante la API.", id);
                TempData["Error"] = "No se pudo eliminar el pago.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
        {
            var dto = await _api.GetPaymentAsync(id, cancellationToken);
            return dto == null ? NotFound() : View(ToEntity(dto));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
        {
            var dto = await _api.GetPaymentAsync(id, cancellationToken);
            return dto == null ? NotFound() : View(ToEntity(dto));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var dto = await _api.GetPaymentAsync(id, cancellationToken);
            return dto == null ? NotFound() : View(ToEntity(dto));
        }

        private static RealEase.Domain.Entities.Payment ToEntity(PaymentDto dto) => new()
        {
            Id = dto.Id, ContractId = dto.ContractId, TenantId = dto.TenantId,
            PaymentDate = dto.PaymentDate, Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod, Status = dto.Status
        };
    }
}
