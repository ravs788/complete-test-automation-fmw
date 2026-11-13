using Core.TestData.SauceDemo;
using Core.TestData.Todo;

namespace Tests.Unit.TestData;

public class MotherTests
{
    [Fact]
    public void UserMother_Should_UseTheSharedDemoPassword()
    {
        Assert.Equal("standard_user", UserMother.Standard().Username);
        Assert.Equal("locked_out_user", UserMother.LockedOut().Username);
        Assert.All(new[] { UserMother.Standard(), UserMother.LockedOut() }, u => Assert.Equal(UserMother.Password, u.Password));
    }

    [Fact]
    public void ProductMother_Should_ReturnDistinctProducts()
    {
        Assert.NotEqual(ProductMother.Backpack().Name, ProductMother.BikeLight().Name);
    }

    [Fact]
    public void TodoItemMother_Should_GiveUniqueTitles_And_MarkCompleted()
    {
        Assert.NotEqual(TodoItemMother.Standard().Title, TodoItemMother.Standard().Title);
        Assert.False(TodoItemMother.Standard().IsCompleted);
        Assert.True(TodoItemMother.Completed().IsCompleted);
    }

    [Fact]
    public void CheckoutInfoBuilder_Should_FillEveryField()
    {
        var info = CheckoutInfoMother.Standard();

        Assert.False(string.IsNullOrEmpty(info.FirstName));
        Assert.False(string.IsNullOrEmpty(info.LastName));
        Assert.False(string.IsNullOrEmpty(info.PostalCode));
    }
}
