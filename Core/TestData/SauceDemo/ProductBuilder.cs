using Core.Models.SauceDemo;

namespace Core.TestData.SauceDemo;

/// <summary>Builds <see cref="Product"/>s; defaults to the backpack.</summary>
public sealed class ProductBuilder
{
    private string _name = "Sauce Labs Backpack";

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Product Build() => new() { Name = _name };
}
