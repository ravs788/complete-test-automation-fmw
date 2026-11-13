using Core.Models.RestfulBooker;

namespace Core.TestData.RestfulBooker;

/// <summary>
/// Builds valid <see cref="Booking"/>s. Defaults: a unique last name (so parallel tests and the shared demo API
/// never collide), a stay starting 30 days from today, deposit paid.
/// </summary>
public sealed class BookingBuilder
{
    private string _firstName = "Test";
    private string _lastName = $"Booking-{Guid.NewGuid():N}";
    private int _totalPrice = 150;
    private bool _depositPaid = true;
    private DateOnly _checkIn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
    private int _nights = 3;
    private string _additionalNeeds = "Breakfast";

    public BookingBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public BookingBuilder WithTotalPrice(int totalPrice)
    {
        _totalPrice = totalPrice;
        return this;
    }

    public BookingBuilder WithDepositPaid(bool depositPaid)
    {
        _depositPaid = depositPaid;
        return this;
    }

    public BookingBuilder WithStay(DateOnly checkIn, int nights)
    {
        _checkIn = checkIn;
        _nights = nights;
        return this;
    }

    public BookingBuilder WithAdditionalNeeds(string additionalNeeds)
    {
        _additionalNeeds = additionalNeeds;
        return this;
    }

    public Booking Build() => new()
    {
        FirstName = _firstName,
        LastName = _lastName,
        TotalPrice = _totalPrice,
        DepositPaid = _depositPaid,
        BookingDates = new BookingDates
        {
            CheckIn = _checkIn.ToString("yyyy-MM-dd"),
            CheckOut = _checkIn.AddDays(_nights).ToString("yyyy-MM-dd")
        },
        AdditionalNeeds = _additionalNeeds
    };
}
