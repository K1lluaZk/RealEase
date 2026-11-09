using System;

namespace RealEase.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

[Table("visits")]
public class Visit
    {
        public long Id { get; set; }
        public long PropertyId { get; set; }
        public Guid UserId { get; set; } // ¡Agregado!
        public DateTime VisitDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
