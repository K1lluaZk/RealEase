using System;

namespace RealEase.Application.Dtos.Visit
{
    public class VisitDto
    {
        public long Id { get; set; }
        public long PropertyId { get; set; }
        public Guid UserId { get; set; } // Ajustado a Guid para la relación con Users
        public DateTime VisitDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
