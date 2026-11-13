using Core.Api;

namespace Tests.Unit.Api;

public class ApiExceptionTests
{
    [Fact]
    public void ApiException_Should_CarryTheStatusCode_And_DescribeTheCall()
    {
        var ex = new ApiException("PUT", "booking/1", 403, "Forbidden");

        Assert.Equal(403, ex.StatusCode);
        Assert.Equal("PUT booking/1 failed: 403 Forbidden", ex.Message);
    }
}
