using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using API.Utilities;
using API.Models;
using Core.Utilities;
using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace API.Tests
{
    [AllureNUnit]
    [AllureSuite("Booking Delete API")]
    [AllureTag("api", "delete", "regression")]
    public class BookingApiDeleteTests : BaseApiTest
    {



        [Test]
        public async Task Delete_Booking_Then_Verify_Should_Fail_To_Get()
        {
            // 1. Create
            var booking = TestDataLoader.Instance.Load<Booking>(
                "BookingApiDeleteTests/Delete.json"
            );
            var postResp = await _client.PostAsync<Booking, Dictionary<string, object>>("booking", booking);
            AllureApi.Step("Assert booking creation for delete test", () =>
            {
                try
                {
                    Assert.That(postResp, Is.Not.Null, "Booking creation failed");
                    Assert.That(postResp.ContainsKey("bookingid"), "Booking creation missing id");
                    Logger.Info("PASSED: Assert booking creation for delete test");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"FAILED: Assert booking creation for delete test - {ex.Message}");
                    throw;
                }
            });
            int bookingId = int.Parse(postResp["bookingid"].ToString()!);

            // 2. Auth
            var token = await AuthHelper.GetAuthTokenAsync();

            // 3. Delete
            var deleteResponse = await _client.DeleteAsync($"booking/{bookingId}", token);
            AllureApi.Step("Assert delete booking response", () =>
            {
                try
                {
                    Assert.That(deleteResponse.IsSuccessful, $"Delete failed: {(int)deleteResponse.StatusCode} {deleteResponse.StatusDescription}");
                    Logger.Info("PASSED: Assert delete booking response");
                }
                catch (AssertionException ex)
                {
                    Logger.Error($"FAILED: Assert delete booking response - {ex.Message}");
                    throw;
                }
            });

            // 4. Try to GET the deleted booking (should 404)
            AllureApi.Step("Assert deleted booking is not found (404)", () =>
            {
                try
                {
                    var afterDelete = _client.GetAsync<Booking>($"booking/{bookingId}").GetAwaiter().GetResult();
                    AllureApi.Step("Fail if booking still exists after delete", () =>
                    {
                        try
                        {
                            Assert.Fail("Booking should not exist after delete, but was found.");
                        }
                        catch (AssertionException failEx)
                        {
                            Logger.Error($"FAILED: Deleted booking was still found after delete - {failEx.Message}");
                            throw;
                        }
                    });
                }
                catch (RestClientException ex)
                {
                    try
                    {
                        Assert.That(ex.Message.Contains("404"), "Expected 404 after deleting booking.");
                        Logger.Info("PASSED: Deleted booking is not found (404 returned as expected)");
                    }
                    catch (AssertionException failEx)
                    {
                        Logger.Error($"FAILED: Did not get 404 after deleted booking - {failEx.Message}");
                        throw;
                    }
                }
            });
        }
    }
}
