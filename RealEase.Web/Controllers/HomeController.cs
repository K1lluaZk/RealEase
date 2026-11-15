using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RealEase.Web.Models;
using RealEase.Web.Services;
using RealEase.Application.Dtos.Property;
using RealEase.Application.Dtos.AgentInquiry;

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

        [AllowAnonymous]
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

        [AllowAnonymous]
        public async Task<IActionResult> Buy(CancellationToken cancellationToken)
        {
            return await Listings("sale", cancellationToken);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Rent(CancellationToken cancellationToken)
        {
            return await Listings("rent", cancellationToken);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Sell()
        {
            return View(new AgentInquiryDto { Intent = "sell" });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactAgent(
            AgentInquiryDto inquiry,
            string? returnUrl,
            CancellationToken cancellationToken)
        {
            var destination = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action(nameof(Sell))!;
            if (!ModelState.IsValid)
            {
                TempData["InquiryError"] = "Revisa los datos del formulario e inténtalo de nuevo.";
                return Redirect(destination);
            }

            try
            {
                var agent = await _api.PostAsync<AgentInquiryDto, AgentInquiryResponseDto>(
                    "api/AgentInquiries",
                    inquiry,
                    cancellationToken);
                TempData["InquiryAgentName"] = agent.AgentName;
                TempData["InquiryAgentEmail"] = agent.AgentEmail;
                TempData["InquiryAgentPhone"] = agent.AgentPhoneNumber;
                TempData["InquirySuccess"] = "Tu solicitud quedó registrada. Puedes contactar directamente a este agente:";
            }
            catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["InquiryError"] = "Aún no hay un agente asignado a esa ubicación. Prueba con otra zona o vuelve a intentarlo más tarde.";
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo enviar la solicitud al agente para {Location}.", inquiry.Location);
                TempData["InquiryError"] = "No fue posible enviar la solicitud ahora. Inténtalo más tarde.";
            }
            catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(exception, "La solicitud al agente para {Location} excedió el tiempo de espera.", inquiry.Location);
                TempData["InquiryError"] = "El servicio está tardando demasiado. Inténtalo de nuevo más tarde.";
            }

            return Redirect(destination);
        }

        [AllowAnonymous]
        public IActionResult Benefits()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Terms()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Security()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<IActionResult> Listings(string listingType, CancellationToken cancellationToken)
        {
            try
            {
                var properties = await _api.GetPropertiesAsync(cancellationToken);
                var listings = properties
                    .Where(property => string.Equals(property.ListingType, listingType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                ViewData["ListingType"] = listingType;
                return View("Listings", listings);
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                _logger.LogWarning(exception, "No se pudieron cargar propiedades de tipo {ListingType}.", listingType);
                ViewData["ListingType"] = listingType;
                ViewData["ApiError"] = "No se pudieron cargar los datos. Inténtalo de nuevo más tarde.";
                return View("Listings", new List<PropertyDto>());
            }
        }
    }
}
