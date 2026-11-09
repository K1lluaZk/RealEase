using Microsoft.AspNetCore.Mvc;
using RealEase.Web.Services;
using RealEase.Application.Dtos.Contract;

namespace RealEase.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly RealEaseApiClient _api;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(RealEaseApiClient api, ILogger<ContractsController> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                return View(await _api.GetContractsAsync(cancellationToken));
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                _logger.LogWarning(exception, "No se pudieron cargar contratos desde la API.");
                ViewData["ApiError"] = "No se pudieron cargar los datos. Comprueba que RealEase.API esté activa y que su conexión a la base de datos funcione.";
                return View(new List<ContractDto>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFromApi(long id, CancellationToken cancellationToken)
        {
            try
            {
                await _api.DeleteAsync($"api/Contracts/{id}", cancellationToken);
                TempData["Success"] = "Contrato eliminado.";
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo eliminar el contrato {ContractId} mediante la API.", id);
                TempData["Error"] = "No se pudo eliminar el contrato. Comprueba si tiene pagos asociados.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
        {
            var dto = await _api.GetContractAsync(id, cancellationToken);
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
            var dto = await _api.GetContractAsync(id, cancellationToken);
            return dto == null ? NotFound() : View(ToEntity(dto));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var dto = await _api.GetContractAsync(id, cancellationToken);
            return dto == null ? NotFound() : View(ToEntity(dto));
        }

        private static RealEase.Domain.Entities.Contract ToEntity(ContractDto dto) => new()
        {
            Id = dto.Id, ClientId = dto.ClientId, AgentId = dto.AgentId,
            PropertyId = dto.PropertyId, StartDate = dto.StartDate,
            EndDate = dto.EndDate, TotalAmount = dto.TotalAmount, Status = dto.Status
        };
    }
}
