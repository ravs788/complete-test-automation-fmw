using System.Text.Json.Serialization;

namespace Core.Models.RestfulBooker;

/// <summary>Response of POST /booking.</summary>
public sealed record CreatedBooking
{
    [JsonPropertyName("bookingid")] public int BookingId { get; init; }
    [JsonPropertyName("booking")] public Booking Booking { get; init; } = new();
}
