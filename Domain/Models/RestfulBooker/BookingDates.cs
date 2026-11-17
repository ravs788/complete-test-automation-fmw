using System.Text.Json.Serialization;

namespace Domain.Models.RestfulBooker;

public sealed record BookingDates
{
    [JsonPropertyName("checkin")] public string CheckIn { get; init; } = "";
    [JsonPropertyName("checkout")] public string CheckOut { get; init; } = "";
}
