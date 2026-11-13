using System.Text.Json.Serialization;

namespace Core.Models.RestfulBooker;

/// <summary>A booking as sent to and returned by Restful Booker (https://restful-booker.herokuapp.com/apidoc).</summary>
public sealed record Booking
{
    [JsonPropertyName("firstname")] public string FirstName { get; init; } = "";
    [JsonPropertyName("lastname")] public string LastName { get; init; } = "";
    [JsonPropertyName("totalprice")] public int TotalPrice { get; init; }
    [JsonPropertyName("depositpaid")] public bool DepositPaid { get; init; }
    [JsonPropertyName("bookingdates")] public BookingDates BookingDates { get; init; } = new();
    [JsonPropertyName("additionalneeds")] public string AdditionalNeeds { get; init; } = "";
}
