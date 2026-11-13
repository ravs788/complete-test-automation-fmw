using Core.Models.SauceDemo;

namespace Core.TestData.SauceDemo;

/// <summary>Products from the SauceDemo catalogue (Object Mother).</summary>
public static class ProductMother
{
    public static Product Backpack() => new ProductBuilder().Build();

    public static Product BikeLight() => new ProductBuilder().WithName("Sauce Labs Bike Light").Build();
}
