using Core.Models.SauceDemo;

namespace Core.TestData.SauceDemo;

/// <summary>Builds valid <see cref="CheckoutInfo"/>; every field is filled by default.</summary>
public sealed class CheckoutInfoBuilder
{
    private string _firstName = "John";
    private string _lastName = "Doe";
    private string _postalCode = "12345";

    public CheckoutInfoBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public CheckoutInfoBuilder WithPostalCode(string postalCode)
    {
        _postalCode = postalCode;
        return this;
    }

    public CheckoutInfo Build() => new() { FirstName = _firstName, LastName = _lastName, PostalCode = _postalCode };
}
