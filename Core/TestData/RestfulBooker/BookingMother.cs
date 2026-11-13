using Core.Models.RestfulBooker;

namespace Core.TestData.RestfulBooker;

/// <summary>Ready-made <see cref="Booking"/>s for common scenarios (Object Mother).</summary>
public static class BookingMother
{
    public static Booking Standard() => new BookingBuilder().Build();

    /// <summary>A booking that differs from <see cref="Standard"/> in every field, for full updates (PUT).</summary>
    public static Booking FullyChanged() => new BookingBuilder()
        .WithName("Updated", $"Person-{Guid.NewGuid():N}")
        .WithTotalPrice(222)
        .WithDepositPaid(false)
        .WithStay(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(60), 4)
        .WithAdditionalNeeds("Dinner")
        .Build();
}
