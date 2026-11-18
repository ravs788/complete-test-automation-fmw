using Domain.Models.RestfulBooker;
using Domain.TestData.RestfulBooker;

namespace Tests.Unit.TestData;

public class BookingPatchTests
{
    [Fact]
    public void ApplyTo_Should_ChangeOnlyTheFieldsThatAreSet()
    {
        // Arrange
        var booking = BookingMother.Standard();

        // Act
        var patched = new BookingPatch { FirstName = "Patched", TotalPrice = 1 }.ApplyTo(booking);

        // Assert
        Assert.Equal(booking with { FirstName = "Patched", TotalPrice = 1 }, patched);
    }

    [Fact]
    public void ApplyTo_Should_ReturnAnEqualBooking_When_PatchIsEmpty()
    {
        // Arrange
        var booking = BookingMother.Standard();

        // Act & Assert
        Assert.Equal(booking, new BookingPatch().ApplyTo(booking));
    }
}
