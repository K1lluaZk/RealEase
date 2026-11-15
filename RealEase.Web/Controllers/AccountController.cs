using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEase.Application.Dtos.Supabase;
using RealEase.Web.Models;
using RealEase.Web.Services;

namespace RealEase.Web.Controllers;

public sealed class AccountController(
    RealEaseApiClient api,
    ILogger<AccountController> logger) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var result = await api.PostAsync<LoginUserRequest, LoginResponseModel>(
                "api/Auth/login",
                new LoginUserRequest { Email = model.Email, Password = model.Password },
                cancellationToken);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new Claim(ClaimTypes.Name, result.FullName),
                new Claim(ClaimTypes.Email, result.Email),
                new Claim(ClaimTypes.Role, result.Role),
                new Claim("access_token", result.AccessToken)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = false,
                    ExpiresUtc = result.ExpiresAt
                });

            return RedirectToLocal(model.ReturnUrl);
        }
        catch (HttpRequestException exception)
        {
            if (exception.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos, o la cuenta no está activa.");
                return View(model);
            }

            logger.LogWarning(exception, "No se pudo completar el inicio de sesión.");
            ModelState.AddModelError(string.Empty, "No fue posible conectar con el servicio de acceso. Inténtalo más tarde.");
            return View(model);
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Home");
}
