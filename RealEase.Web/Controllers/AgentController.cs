using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEase.Application.Dtos.Contract;
using RealEase.Application.Dtos.Property;
using RealEase.Application.Dtos.Visit;
using RealEase.Domain.Models;
using RealEase.Persistence.Context;
using RealEase.Web.Services;
using RealEase.Web.ViewsModels;

namespace RealEase.Web.Controllers
{
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Agent)]
    public class AgentController : Controller
    {
        private readonly RealEaseDbContext _dbContext;
        private readonly RealEaseApiClient _api;
        private readonly ILogger<AgentController> _logger;

        public AgentController(
            RealEaseDbContext dbContext,
            RealEaseApiClient api,
            ILogger<AgentController> logger)
        {
            _dbContext = dbContext;
            _api = api;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdValue, out var agentId);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var agentUser = await _dbContext.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == agentId, cancellationToken);

            // Inquiries assigned to this agent (or all if admin)
            var inquiriesQuery = _dbContext.AgentInquiries.AsNoTracking();
            if (!isAdmin && agentId != Guid.Empty)
            {
                inquiriesQuery = inquiriesQuery.Where(i => i.AgentId == agentId);
            }
            var inquiries = await inquiriesQuery
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(cancellationToken);

            // Fetch API resources safely
            List<VisitDto> allVisits = new();
            List<ContractDto> allContracts = new();
            List<PropertyDto> allProperties = new();

            try
            {
                allVisits = await _api.GetVisitsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron obtener visitas para el dashboard de agente.");
            }

            try
            {
                allContracts = await _api.GetContractsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron obtener contratos para el dashboard de agente.");
            }

            try
            {
                allProperties = await _api.GetPropertiesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron obtener propiedades para el dashboard de agente.");
            }

            // Filter contracts for this agent if not admin
            var agentContracts = isAdmin
                ? allContracts
                : allContracts.Where(c => c.AgentId == agentId).ToList();

            var pendingInquiries = inquiries.Count(i => string.Equals(i.Status, "pending", StringComparison.OrdinalIgnoreCase));
            var upcomingVisits = allVisits
                .Where(v => v.VisitDate >= DateTime.UtcNow && !string.Equals(v.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
                .OrderBy(v => v.VisitDate)
                .Take(5)
                .ToList();

            var activeContracts = agentContracts
                .Where(c => string.Equals(c.Status, "active", StringComparison.OrdinalIgnoreCase) || string.Equals(c.Status, "signed", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var totalVolume = agentContracts.Sum(c => c.TotalAmount);

            var viewModel = new AgentDashboardViewModel
            {
                AgentName = agentUser != null ? $"{agentUser.FirstName} {agentUser.LastName}".Trim() : (User.Identity?.Name ?? "Agente Inmobiliario"),
                AgentEmail = agentUser?.Email ?? (User.FindFirstValue(ClaimTypes.Email) ?? string.Empty),
                ServiceArea = agentUser?.ServiceArea ?? "República Dominicana",
                PendingInquiriesCount = pendingInquiries,
                UpcomingVisitsCount = upcomingVisits.Count,
                ActiveContractsCount = activeContracts.Count,
                TotalVolume = totalVolume,
                RecentInquiries = inquiries.Take(6).ToList(),
                UpcomingVisits = upcomingVisits,
                RecentContracts = agentContracts.OrderByDescending(c => c.StartDate).Take(5).ToList(),
                ManagedProperties = allProperties.Take(6).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Inquiries(string? status, CancellationToken cancellationToken)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdValue, out var agentId);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            var query = _dbContext.AgentInquiries.AsNoTracking();
            if (!isAdmin && agentId != Guid.Empty)
            {
                query = query.Where(i => i.AgentId == agentId);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                query = query.Where(i => i.Status.ToLower() == status.ToLower());
            }

            var inquiries = await query
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(cancellationToken);

            ViewData["CurrentStatus"] = status ?? "all";
            return View(inquiries);
        }

        [HttpGet]
        public async Task<IActionResult> Visits(CancellationToken cancellationToken)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdValue, out var agentId);
            var isAdmin = User.IsInRole(UserRoles.Admin);

            List<VisitDto> allVisits = new();
            try
            {
                allVisits = await _api.GetVisitsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron obtener visitas para la agenda del agente.");
            }

            // Admins see all; agents see only visits where their properties are involved
            // Since VisitDto may not have an AgentId, we show all to agents (filtered by upcoming)
            var now = DateTime.UtcNow;
            var upcoming = allVisits
                .Where(v => v.VisitDate >= now && !string.Equals(v.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
                .OrderBy(v => v.VisitDate)
                .ToList();

            var past = allVisits
                .Where(v => v.VisitDate < now || string.Equals(v.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(v => v.VisitDate)
                .ToList();

            ViewData["UpcomingVisits"] = upcoming;
            ViewData["PastVisits"] = past;
            ViewData["TotalCount"] = allVisits.Count;
            ViewData["UpcomingCount"] = upcoming.Count;

            return View(allVisits);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInquiryStatus(
            long id, 
            string status, 
            string? returnUrl, 
            CancellationToken cancellationToken)
        {
            var inquiry = await _dbContext.AgentInquiries.FindAsync(new object[] { id }, cancellationToken);
            if (inquiry == null)
            {
                TempData["Error"] = "No se encontró la solicitud.";
                return Redirect(returnUrl ?? Url.Action(nameof(Dashboard))!);
            }

            inquiry.Status = status;
            await _dbContext.SaveChangesAsync(cancellationToken);

            TempData["Success"] = $"El estado de la solicitud de {inquiry.Name} fue actualizado a '{status}'.";
            return Redirect(returnUrl ?? Url.Action(nameof(Dashboard))!);
        }
    }
}
