using RealEase.Domain.Entities;
using RealEase.Application.Dtos.Visit;
using RealEase.Application.Dtos.Contract;
using RealEase.Application.Dtos.Property;

namespace RealEase.Web.ViewsModels
{
    public class AgentDashboardViewModel
    {
        public string AgentName { get; set; } = string.Empty;
        public string AgentEmail { get; set; } = string.Empty;
        public string? ServiceArea { get; set; }

        public int PendingInquiriesCount { get; set; }
        public int UpcomingVisitsCount { get; set; }
        public int ActiveContractsCount { get; set; }
        public decimal TotalVolume { get; set; }

        public List<AgentInquiry> RecentInquiries { get; set; } = new();
        public List<VisitDto> UpcomingVisits { get; set; } = new();
        public List<ContractDto> RecentContracts { get; set; } = new();
        public List<PropertyDto> ManagedProperties { get; set; } = new();
    }
}
