using Core.Api;

namespace Tests.Unit.Api;

public class ApiExceptionTests
{
    [Fact]
    public void ApiException_Should_CarryTheStatusCode_And_DescribeTheCall()
    {
        // Arrange: use a PUT call that failed with HTTP 403.
        // Act
        var ex = new ApiException("PUT", "booking/1", 403, "Forbidden");

        // Assert
        Assert.Equal(403, ex.StatusCode);
        Assert.Equal("PUT booking/1 failed: 403 Forbidden", ex.Message);
    }
}
