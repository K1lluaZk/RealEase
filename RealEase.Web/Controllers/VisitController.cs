using Microsoft.AspNetCore.Mvc;
using RealEase.Web.Services;
using RealEase.Application.Dtos.Visit;

namespace RealEase.Web.Controllers
{
    public class VisitsController : Controller
    {
        private readonly RealEaseApiClient _api;
        private readonly ILogger<VisitsController> _logger;

        public VisitsController(RealEaseApiClient api, ILogger<VisitsController> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                return View(await _api.GetVisitsAsync(cancellationToken));
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                _logger.LogWarning(exception, "No se pudieron cargar visitas desde la API.");
                ViewData["ApiError"] = "No se pudieron cargar los datos. Comprueba que RealEase.API esté activa y que su conexión a la base de datos funcione.";
                return View(new List<VisitDto>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFromApi(long id, CancellationToken cancellationToken)
        {
            try
            {
                await _api.DeleteAsync($"api/Visits/{id}", cancellationToken);
                TempData["Success"] = "Visita eliminada.";
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo eliminar la visita {VisitId} mediante la API.", id);
                TempData["Error"] = "No se pudo eliminar la visita.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
        {
            var dto = await _api.GetVisitAsync(id, cancellationToken);
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
            var dto = await _api.GetVisitAsync(id, cancellationToken);
            return dto == null ? NotFound() : View(ToEntity(dto));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var dto = await _api.GetVisitAsync(id, cancellationToken);
            return dto == null ? NotFound() : View(ToEntity(dto));
        }

        private static RealEase.Domain.Entities.Visit ToEntity(VisitDto dto) => new()
        {
            Id = dto.Id, PropertyId = dto.PropertyId, UserId = dto.UserId,
            VisitDate = dto.VisitDate, Status = dto.Status, Notes = dto.Notes,
            CreatedAt = dto.CreatedAt, UpdatedAt = dto.UpdatedAt
        };
    }
}
