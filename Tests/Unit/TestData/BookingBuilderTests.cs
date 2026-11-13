using Core.TestData.RestfulBooker;

namespace Tests.Unit.TestData;

public class BookingBuilderTests
{
    [Fact]
    public void Build_Should_GiveEachBookingAUniqueLastName()
    {
        var names = Enumerable.Range(0, 20).Select(_ => new BookingBuilder().Build().LastName).ToList();

        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact]
    public void Build_Should_CreateAFutureStay_When_NoDatesGiven()
    {
        var dates = new BookingBuilder().Build().BookingDates;
        var checkIn = DateOnly.Parse(dates.CheckIn);
        var checkOut = DateOnly.Parse(dates.CheckOut);

        Assert.True(checkIn > DateOnly.FromDateTime(DateTime.UtcNow));
        Assert.Equal(3, checkOut.DayNumber - checkIn.DayNumber);
    }

    [Fact]
    public void WithStay_Should_SetCheckInAndCheckOut()
    {
        var dates = new BookingBuilder().WithStay(new DateOnly(2030, 1, 30), 4).Build().BookingDates;

        Assert.Equal("2030-01-30", dates.CheckIn);
        Assert.Equal("2030-02-03", dates.CheckOut);
    }

    [Fact]
    public void FullyChanged_Should_DifferFromStandardInEveryField()
    {
        var standard = BookingMother.Standard();
        var changed = BookingMother.FullyChanged();

        Assert.NotEqual(standard.FirstName, changed.FirstName);
        Assert.NotEqual(standard.LastName, changed.LastName);
        Assert.NotEqual(standard.TotalPrice, changed.TotalPrice);
        Assert.NotEqual(standard.DepositPaid, changed.DepositPaid);
        Assert.NotEqual(standard.BookingDates, changed.BookingDates);
        Assert.NotEqual(standard.AdditionalNeeds, changed.AdditionalNeeds);
    }
}
