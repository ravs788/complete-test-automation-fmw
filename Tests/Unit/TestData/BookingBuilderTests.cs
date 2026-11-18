using Domain.TestData.RestfulBooker;

namespace Tests.Unit.TestData;

public class BookingBuilderTests
{
    [Fact]
    public void Build_Should_GiveEachBookingAUniqueLastName()
    {
        // Arrange: use independent builders with their default settings.
        // Act
        var names = Enumerable.Range(0, 20).Select(_ => new BookingBuilder().Build().LastName).ToList();

        // Assert
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact]
    public void Build_Should_CreateAFutureStay_When_NoDatesGiven()
    {
        // Arrange: use a booking builder with its default stay.
        // Act
        var dates = new BookingBuilder().Build().BookingDates;
        var checkIn = DateOnly.Parse(dates.CheckIn);
        var checkOut = DateOnly.Parse(dates.CheckOut);

        // Assert
        Assert.True(checkIn > DateOnly.FromDateTime(DateTime.UtcNow));
        Assert.Equal(3, checkOut.DayNumber - checkIn.DayNumber);
    }

    [Fact]
    public void WithStay_Should_SetCheckInAndCheckOut()
    {
        // Arrange: use a fixed check-in date and a four-day stay.
        // Act
        var dates = new BookingBuilder().WithStay(new DateOnly(2030, 1, 30), 4).Build().BookingDates;

        // Assert
        Assert.Equal("2030-01-30", dates.CheckIn);
        Assert.Equal("2030-02-03", dates.CheckOut);
    }

    [Fact]
    public void FullyChanged_Should_DifferFromStandardInEveryField()
    {
        // Arrange
        var standard = BookingMother.Standard();

        // Act
        var changed = BookingMother.FullyChanged();

        // Assert
        Assert.NotEqual(standard.FirstName, changed.FirstName);
        Assert.NotEqual(standard.LastName, changed.LastName);
        Assert.NotEqual(standard.TotalPrice, changed.TotalPrice);
        Assert.NotEqual(standard.DepositPaid, changed.DepositPaid);
        Assert.NotEqual(standard.BookingDates, changed.BookingDates);
        Assert.NotEqual(standard.AdditionalNeeds, changed.AdditionalNeeds);
    }
}
