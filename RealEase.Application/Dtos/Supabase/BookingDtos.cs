using System;

namespace RealEase.Application.Dtos.Supabase
{
    public class CheckAvailabilityRequest
    {
        public string PropertyId { get; set; } = string.Empty;
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
    }

    public class AvailabilityCheckResult
    {
        public bool IsAvailable { get; set; }
        public int TotalNights { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal TotalEstimatedPrice { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CreateReservationRequest
    {
        public string PropertyId { get; set; } = string.Empty;
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string? Notes { get; set; }
    }

    public class ReservationResponse
    {
        public string Id { get; set; } = string.Empty;
        public string PropertyId { get; set; } = string.Empty;
        public string PropertyTitle { get; set; } = string.Empty;
        public string GuestId { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int TotalNights { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
