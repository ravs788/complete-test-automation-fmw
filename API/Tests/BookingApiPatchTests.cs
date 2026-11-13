using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utilities;
using API.Models;
using Core.Utilities;
using Allure.Net.Commons;
using Allure.Net.Commons.Attributes;
using Xunit;
using Xunit.Sdk;

namespace API.Tests
{
    [AllureSuite("Booking Patch API")]
    [AllureTag("api", "patch", "regression")]
    [Trait("Category", "regression")]
    public class BookingApiPatchTests : BaseApiTest
    {


        [Fact]
        public Task Patch_Firstname_Lastname_Then_Verify() => RunAsync(async () =>
        {
            // 1. Create booking
            var original = TestDataLoader.Instance.Load<Booking>(
                "BookingApiPatchTests/Original.json"
            );
            var postResp = await _client.PostAsync<Booking, Dictionary<string, object>>("booking", original);
            AllureApi.Step("Assert booking created for patch test", () =>
            {
                try
                {
                    Assert.NotNull(postResp);
                    Assert.True(postResp.ContainsKey("bookingid"), "Post response missing id");
                    Logger.Info("PASSED: Assert booking created for patch test");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"FAILED: Assert booking created for patch test - {ex.Message}");
                    throw;
                }
            });
            int bookingId = int.Parse(postResp["bookingid"].ToString()!);

            // 2. Auth
            var token = await AuthHelper.GetAuthTokenAsync();

            // 3. PATCH: load partial update data from test-data
            var patchData = TestDataLoader.Instance.Load<Dictionary<string, object>>(
                "BookingApiPatchTests/Patch.json"
            );
            var patchResp = await _client.PatchAsync<Dictionary<string, object>, Booking>($"booking/{bookingId}", patchData, token);

            AllureApi.Step("Assert patchResp is not null", () =>
            {
                try
                {
                    Assert.NotNull(patchResp);
                    Logger.Info("PASSED: Assert patchResp is not null");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"FAILED: Assert patchResp is not null - {ex.Message}");
                    throw;
                }
            });

            AllureApi.Step("Assert firstname was patched correctly", () =>
            {
                try
                {
                    Assert.Equal(patchData["firstname"].ToString(), patchResp.firstname?.Replace("\"", ""));
                    Logger.Info("PASSED: Assert firstname was patched correctly");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"FAILED: Assert firstname was patched correctly - {ex.Message}");
                    throw;
                }
            });
            AllureApi.Step("Assert lastname was patched correctly", () =>
            {
                try
                {
                    Assert.Equal(patchData["lastname"].ToString(), patchResp.lastname?.Replace("\"", ""));
                    Logger.Info("PASSED: Assert lastname was patched correctly");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"FAILED: Assert lastname was patched correctly - {ex.Message}");
                    throw;
                }
            });
            AllureApi.Step("Assert original fields are unchanged", () =>
            {
                try
                {
                    Assert.Equal(original.totalprice, patchResp.totalprice);
                    Assert.Equal(original.depositpaid, patchResp.depositpaid);
                    Assert.Equal(original.bookingdates.checkin, patchResp.bookingdates.checkin);
                    Assert.Equal(original.bookingdates.checkout, patchResp.bookingdates.checkout);
                    Assert.Equal(original.additionalneeds, patchResp.additionalneeds);
                    Logger.Info("PASSED: Assert original fields are unchanged after patch");
                }
                catch (XunitException ex)
                {
                    Logger.Error($"FAILED: Assert original fields are unchanged after patch - {ex.Message}");
                    throw;
                }
            });
        });
    }
}
