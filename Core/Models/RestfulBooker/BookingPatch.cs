using System.Text.Json.Serialization;

namespace Core.Models.RestfulBooker;

/// <summary>Body of PATCH /booking/{id}: only the fields that are set are sent.</summary>
public sealed record BookingPatch
{
    [JsonPropertyName("firstname")] public string? FirstName { get; init; }
    [JsonPropertyName("lastname")] public string? LastName { get; init; }
    [JsonPropertyName("totalprice")] public int? TotalPrice { get; init; }
    [JsonPropertyName("depositpaid")] public bool? DepositPaid { get; init; }
    [JsonPropertyName("bookingdates")] public BookingDates? BookingDates { get; init; }
    [JsonPropertyName("additionalneeds")] public string? AdditionalNeeds { get; init; }

    /// <summary>Returns <paramref name="booking"/> with the fields of this patch applied.</summary>
    public Booking ApplyTo(Booking booking) => booking with
    {
        FirstName = FirstName ?? booking.FirstName,
        LastName = LastName ?? booking.LastName,
        TotalPrice = TotalPrice ?? booking.TotalPrice,
        DepositPaid = DepositPaid ?? booking.DepositPaid,
        BookingDates = BookingDates ?? booking.BookingDates,
        AdditionalNeeds = AdditionalNeeds ?? booking.AdditionalNeeds
    };
}
