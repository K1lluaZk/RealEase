using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RealEase.Web.Models;
using RealEase.Web.Services;
using RealEase.Application.Dtos.Property;

namespace RealEase.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RealEaseApiClient _api;

        public HomeController(ILogger<HomeController> logger, RealEaseApiClient api)
        {
            _logger = logger;
            _api = api;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                return View(await _api.GetPropertiesAsync(cancellationToken));
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                _logger.LogWarning(exception, "No se pudieron cargar propiedades para la página de inicio.");
                ViewData["ApiError"] = "No se pudieron cargar los datos. Comprueba que RealEase.API esté activa y que su conexión a la base de datos funcione.";
                return View(new List<PropertyDto>());
            }
        }

        public IActionResult Benefits()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
