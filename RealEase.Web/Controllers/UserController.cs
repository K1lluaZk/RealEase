using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RealEase.Domain.Models;
using RealEase.Application.Dtos.User;
using RealEase.Persistence.Context;
using RealEase.Web.Services;
using System;
using System.Linq;
using System.Text;

namespace RealEase.Web.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly RealEaseDbContext _context;
        private readonly RealEaseApiClient _api;
        private readonly ILogger<UsersController> _logger;

        public UsersController(RealEaseDbContext context, RealEaseApiClient api, ILogger<UsersController> logger)
        {
            _context = context;
            _api = api;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(Guid id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            user.Role = UserRoles.Normalize(user.Role) ?? user.Role;
            return View(user);
        }

        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> ListFromApi(CancellationToken cancellationToken)
        {
            try
            {
                return Json(await _api.GetAsync<List<UserDto>>("api/Users", cancellationToken));
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo cargar la lista de usuarios mediante la API.");
                return StatusCode((int)(exception.StatusCode ?? System.Net.HttpStatusCode.BadGateway));
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterFromApi(
            string? name,
            string? role,
            bool? isActive,
            CancellationToken cancellationToken)
        {
            var query = new StringBuilder("api/Users/filter");
            var values = new List<string>();
            if (!string.IsNullOrWhiteSpace(name))
                values.Add($"name={Uri.EscapeDataString(name)}");
            if (!string.IsNullOrWhiteSpace(role))
                values.Add($"role={Uri.EscapeDataString(role)}");
            if (isActive.HasValue)
                values.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");
            if (values.Count > 0)
                query.Append('?').AppendJoin('&', values);

            try
            {
                return Json(await _api.GetAsync<List<UserDto>>(query.ToString(), cancellationToken));
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo filtrar la lista de usuarios mediante la API.");
                return StatusCode((int)(exception.StatusCode ?? System.Net.HttpStatusCode.BadGateway));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromApi([FromBody] UserDto dto, CancellationToken cancellationToken)
        {
            try
            {
                await _api.PostAsync<UserDto, Guid>("api/Users", dto, cancellationToken);
                return Ok();
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo crear el perfil de usuario mediante la API.");
                return StatusCode((int)(exception.StatusCode ?? System.Net.HttpStatusCode.BadGateway));
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFromApi([FromBody] UserDto dto, CancellationToken cancellationToken)
        {
            try
            {
                await _api.PutAsync("api/Users", dto, cancellationToken);
                return NoContent();
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo actualizar el perfil {UserId} mediante la API.", dto.Id);
                return StatusCode((int)(exception.StatusCode ?? System.Net.HttpStatusCode.BadGateway));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFromApi(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _api.DeleteAsync($"api/Users/{id}", cancellationToken);
                return Ok();
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "No se pudo eliminar el perfil {UserId} mediante la API.", id);
                return StatusCode((int)(exception.StatusCode ?? System.Net.HttpStatusCode.BadGateway));
            }
        }
    }
}
