using Core.Models.SauceDemo;

namespace Core.TestData.SauceDemo;

/// <summary>SauceDemo's demo accounts (all share one public password), as an Object Mother.</summary>
public static class UserMother
{
    public const string Password = "secret_sauce";

    public static User Standard() => new UserBuilder().Build();

    public static User LockedOut() => new UserBuilder().WithUsername("locked_out_user").Build();
}
