namespace Core.Api.RestfulBooker;

// Property names match the Restful Booker JSON (https://restful-booker.herokuapp.com/apidoc).

public sealed record Booking
{
    public string firstname { get; init; } = "";
    public string lastname { get; init; } = "";
    public int totalprice { get; init; }
    public bool depositpaid { get; init; }
    public BookingDates bookingdates { get; init; } = new();
    public string additionalneeds { get; init; } = "";
}

public sealed record BookingDates
{
    public string checkin { get; init; } = "";
    public string checkout { get; init; } = "";
}

/// <summary>Response of POST /booking.</summary>
public sealed record CreatedBooking
{
    public int bookingid { get; init; }
    public Booking booking { get; init; } = new();
}

/// <summary>Item of GET /booking.</summary>
public sealed record BookingId
{
    public int bookingid { get; init; }
}

public sealed record AuthRequest
{
    public string username { get; init; } = "";
    public string password { get; init; } = "";
}

public sealed record AuthResponse
{
    public string token { get; init; } = "";
}
