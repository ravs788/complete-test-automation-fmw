using Core.Models.SauceDemo;

namespace Core.TestData.SauceDemo;

/// <summary>Builds <see cref="User"/>s; defaults to the standard demo account.</summary>
public sealed class UserBuilder
{
    private string _username = "standard_user";
    private string _password = UserMother.Password;

    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public UserBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public User Build() => new() { Username = _username, Password = _password };
}
