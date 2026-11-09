using Microsoft.AspNetCore.Mvc;
using RealEase.Web.Services;
using RealEase.Application.Dtos.Property;

namespace RealEase.Web.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly RealEaseApiClient _api;
        private readonly ILogger<PropertiesController> _logger;

        public PropertiesController(RealEaseApiClient api, ILogger<PropertiesController> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                return View(await _api.GetPropertiesAsync(cancellationToken));
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                _logger.LogWarning(exception, "No se pudieron cargar propiedades desde la API.");
                ViewData["ApiError"] = "No se pudieron cargar los datos. Comprueba que RealEase.API esté activa y que su conexión a la base de datos funcione.";
                return View(new List<PropertyDto>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFromApi(long id, CancellationToken cancellationToken)
        {
            try
            {
                await _api.DeleteAsync($"api/Properties/{id}", cancellationToken);
                TempData["Success"] = "Propiedad eliminada.";
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo eliminar la propiedad {PropertyId} mediante la API.", id);
                TempData["Error"] = "No se pudo eliminar la propiedad. Revisa si tiene contratos o visitas asociados.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
        {
            var dto = await _api.GetPropertyAsync(id, cancellationToken);
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
            var dto = await _api.GetPropertyAsync(id, cancellationToken);
            return dto == null ? NotFound() : View(ToEntity(dto));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var dto = await _api.GetPropertyAsync(id, cancellationToken);
            return dto == null ? NotFound() : View(ToEntity(dto));
        }

        private static RealEase.Domain.Entities.Property ToEntity(PropertyDto dto) => new()
        {
            Id = dto.Id, Title = dto.Title, Description = dto.Description,
            Address = dto.Address, PricePerNight = dto.PricePerNight,
            PropertyType = dto.PropertyType, Status = dto.Status, OwnerId = dto.OwnerId,
            Images = dto.Images, Bedrooms = dto.Bedrooms, Bathrooms = dto.Bathrooms,
            MaxGuests = dto.MaxGuests, CreatedAt = dto.CreatedAt, UpdatedAt = dto.UpdatedAt
        };
    }
}

