using Core.Models.SauceDemo;

namespace Core.TestData.SauceDemo;

/// <summary>Ready-made <see cref="CheckoutInfo"/> (Object Mother).</summary>
public static class CheckoutInfoMother
{
    public static CheckoutInfo Standard() => new CheckoutInfoBuilder().Build();
}
