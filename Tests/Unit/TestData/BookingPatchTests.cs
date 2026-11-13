using Core.Models.RestfulBooker;
using Core.TestData.RestfulBooker;

namespace Tests.Unit.TestData;

public class BookingPatchTests
{
    [Fact]
    public void ApplyTo_Should_ChangeOnlyTheFieldsThatAreSet()
    {
        var booking = BookingMother.Standard();

        var patched = new BookingPatch { FirstName = "Patched", TotalPrice = 1 }.ApplyTo(booking);

        Assert.Equal(booking with { FirstName = "Patched", TotalPrice = 1 }, patched);
    }

    [Fact]
    public void ApplyTo_Should_ReturnAnEqualBooking_When_PatchIsEmpty()
    {
        var booking = BookingMother.Standard();

        Assert.Equal(booking, new BookingPatch().ApplyTo(booking));
    }
}
