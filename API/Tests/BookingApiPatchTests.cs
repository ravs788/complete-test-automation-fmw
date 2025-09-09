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
    public class BookingApiPatchTests
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
        public async Task Patch_Firstname_Lastname_Then_Verify()
        {
            // 1. Create booking
            var original = TestDataLoader.Instance.Load<Booking>(
                "BookingApiPatchTests/Original.json"
            );
            var postResp = await _client.PostAsync<Booking, Dictionary<string, object>>("booking", original);
            AllureApi.Step("Assert booking created for patch test", () =>
            {
                Assert.That(postResp, Is.Not.Null);
                Assert.That(postResp.ContainsKey("bookingid"), "Post response missing id");
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
                Assert.That(patchResp, Is.Not.Null);
            });

            AllureApi.Step("Assert firstname was patched correctly", () =>
            {
                Assert.That(patchResp.firstname?.Replace("\"", ""), Is.EqualTo(patchData["firstname"].ToString()));
            });
            AllureApi.Step("Assert lastname was patched correctly", () =>
            {
                Assert.That(patchResp.lastname?.Replace("\"", ""), Is.EqualTo(patchData["lastname"].ToString()));
            });
            AllureApi.Step("Assert original fields are unchanged", () =>
            {
                Assert.That(patchResp.totalprice, Is.EqualTo(original.totalprice));
                Assert.That(patchResp.depositpaid, Is.EqualTo(original.depositpaid));
                Assert.That(patchResp.bookingdates.checkin, Is.EqualTo(original.bookingdates.checkin));
                Assert.That(patchResp.bookingdates.checkout, Is.EqualTo(original.bookingdates.checkout));
                Assert.That(patchResp.additionalneeds, Is.EqualTo(original.additionalneeds));
            });
        }
    }
}
