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
    public class BookingApiDeleteTests
    {
        private ApiClient _client;
        private DateTime _testStartTime;

        [SetUp]
        public void SetUp()
        {
            _testStartTime = DateTime.Now;
            _client = new ApiClient();
        }

        [TearDown]
        public void TearDown()
        {
            DateTime endTime = DateTime.Now;
            AllureLifecycle.Instance.UpdateTestCase(tc =>
            {
                tc.parameters.Add(new Allure.Net.Commons.Parameter
                {
                    name = "Start Time",
                    value = _testStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Allure.Net.Commons.Parameter
                {
                    name = "End Time",
                    value = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
                tc.parameters.Add(new Allure.Net.Commons.Parameter
                {
                    name = "Duration (s)",
                    value = (endTime - _testStartTime).TotalSeconds.ToString("F3")
                });
            });
            _client.Dispose();
        }


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
                Assert.That(postResp, Is.Not.Null, "Booking creation failed");
                Assert.That(postResp.ContainsKey("bookingid"), "Booking creation missing id");
            });
            int bookingId = int.Parse(postResp["bookingid"].ToString()!);

            // 2. Auth
            var token = await AuthHelper.GetAuthTokenAsync();

            // 3. Delete
            var deleteResponse = await _client.DeleteAsync($"booking/{bookingId}", token);
            AllureApi.Step("Assert delete booking response", () =>
            {
                Assert.That(deleteResponse.IsSuccessful, $"Delete failed: {(int)deleteResponse.StatusCode} {deleteResponse.StatusDescription}");
            });

            // 4. Try to GET the deleted booking (should 404)
            AllureApi.Step("Assert deleted booking is not found (404)", () =>
            {
                try
                {
                    var afterDelete = _client.GetAsync<Booking>($"booking/{bookingId}").GetAwaiter().GetResult();
                    AllureApi.Step("Fail if booking still exists after delete", () =>
                    {
                        Assert.Fail("Booking should not exist after delete, but was found.");
                    });
                }
                catch (RestClientException ex)
                {
                    Assert.That(ex.Message.Contains("404"), "Expected 404 after deleting booking.");
                }
            });
        }
    }
}
