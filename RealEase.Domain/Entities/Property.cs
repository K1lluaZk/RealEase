using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEase.Domain.Entities;

[Table("properties")]
public class Property
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public string? Status { get; set; }
    public Guid? OwnerId { get; set; }
    public string[]? Images { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? MaxGuests { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public string? ImageUrl
    {
        get => Images != null && Images.Length > 0 ? Images[0] : null;
        set => Images = string.IsNullOrEmpty(value) ? Array.Empty<string>() : new[] { value };
    }

    [NotMapped]
    public int? Capacity
    {
        get => MaxGuests;
        set => MaxGuests = value;
    }
}
