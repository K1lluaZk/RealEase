using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RealEase.Domain.Models;
using RealEase.Web.Services;
using RealEase.Application.Dtos.Visit;

namespace RealEase.Web.Controllers
{
    [Authorize]
    public class VisitsController : Controller
    {
        private readonly RealEaseApiClient _api;
        private readonly ILogger<VisitsController> _logger;

        public VisitsController(RealEaseApiClient api, ILogger<VisitsController> logger)
        {
            _api = api;
            _logger = logger;
        }

        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Agent + "," + UserRoles.Client)]
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

        [Authorize(Roles = UserRoles.Admin)]
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

        [Authorize(Roles = UserRoles.Client)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromApi([FromBody] VisitDto dto, CancellationToken cancellationToken)
        {
            try
            {
                await _api.PostAsync<VisitDto, long>("api/Visits", dto, cancellationToken);
                return Ok();
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo crear la visita mediante la API.");
                return StatusCode((int)(exception.StatusCode ?? System.Net.HttpStatusCode.BadGateway));
            }
        }

        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Agent)]
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFromApi([FromBody] VisitDto dto, CancellationToken cancellationToken)
        {
            try
            {
                await _api.PutAsync("api/Visits", dto, cancellationToken);
                return NoContent();
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo actualizar la visita {VisitId} mediante la API.", dto.Id);
                return StatusCode((int)(exception.StatusCode ?? System.Net.HttpStatusCode.BadGateway));
            }
        }

        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Agent + "," + UserRoles.Client)]
        public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
        {
            var dto = await _api.GetVisitAsync(id, cancellationToken);
            return dto == null ? NotFound() : View(ToEntity(dto));
        }

        [Authorize(Roles = UserRoles.Client)]
        [HttpGet]
        public async Task<IActionResult> Create(long propertyId, CancellationToken cancellationToken)
        {
            if (propertyId <= 0)
                return BadRequest();

            var property = await _api.GetPropertyAsync(propertyId, cancellationToken);
            if (property is null)
                return NotFound();
            if (!string.Equals(property.ListingType, "sale", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Details", "Properties", new { id = propertyId });

            ViewData["PropertyTitle"] = property.Title;
            return View(new RealEase.Domain.Entities.Visit
            {
                PropertyId = propertyId,
                UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            });
        }

        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Agent)]
        [HttpGet]
        public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
        {
            var dto = await _api.GetVisitAsync(id, cancellationToken);
            return dto == null ? NotFound() : View(ToEntity(dto));
        }

        [Authorize(Roles = UserRoles.Admin)]
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
