using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Utilities;
using API.Models;
using Core.Utilities;
using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace API.Tests
{
    [AllureNUnit]
    [AllureSuite("Booking Patch API")]
    [AllureTag("api", "patch", "regression")]
    [Parallelizable(ParallelScope.All)]
    public class BookingApiPatchTests : BaseApiTest
    {


        [Test]
        public async Task Patch_Firstname_Lastname_Then_Verify()
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
                    Assert.That(postResp, Is.Not.Null);
                    Assert.That(postResp.ContainsKey("bookingid"), "Post response missing id");
                    Logger.Info("PASSED: Assert booking created for patch test");
                }
                catch (AssertionException ex)
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
                    Assert.That(patchResp, Is.Not.Null);
                    Logger.Info("PASSED: Assert patchResp is not null");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"FAILED: Assert patchResp is not null - {ex.Message}");
                    throw;
                }
            });

            AllureApi.Step("Assert firstname was patched correctly", () =>
            {
                try
                {
                    Assert.That(patchResp.firstname?.Replace("\"", ""), Is.EqualTo(patchData["firstname"].ToString()));
                    Logger.Info("PASSED: Assert firstname was patched correctly");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"FAILED: Assert firstname was patched correctly - {ex.Message}");
                    throw;
                }
            });
            AllureApi.Step("Assert lastname was patched correctly", () =>
            {
                try
                {
                    Assert.That(patchResp.lastname?.Replace("\"", ""), Is.EqualTo(patchData["lastname"].ToString()));
                    Logger.Info("PASSED: Assert lastname was patched correctly");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"FAILED: Assert lastname was patched correctly - {ex.Message}");
                    throw;
                }
            });
            AllureApi.Step("Assert original fields are unchanged", () =>
            {
                try
                {
                    Assert.That(patchResp.totalprice, Is.EqualTo(original.totalprice));
                    Assert.That(patchResp.depositpaid, Is.EqualTo(original.depositpaid));
                    Assert.That(patchResp.bookingdates.checkin, Is.EqualTo(original.bookingdates.checkin));
                    Assert.That(patchResp.bookingdates.checkout, Is.EqualTo(original.bookingdates.checkout));
                    Assert.That(patchResp.additionalneeds, Is.EqualTo(original.additionalneeds));
                    Logger.Info("PASSED: Assert original fields are unchanged after patch");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"FAILED: Assert original fields are unchanged after patch - {ex.Message}");
                    throw;
                }
            });
        }
    }
}
