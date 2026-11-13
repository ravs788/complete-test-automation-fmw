using System.Text.Json.Serialization;

namespace Core.Models.RestfulBooker;

/// <summary>Item of GET /booking.</summary>
public sealed record BookingId
{
    [JsonPropertyName("bookingid")] public int Id { get; init; }
}
