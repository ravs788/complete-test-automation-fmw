using Core.Utilities;
using NUnit.Framework;
using System;

namespace UI.Web.Utilities
{
    /// <summary>
    /// Provides custom assertion methods that also capture screenshots on pass/fail.
    /// </summary>
    public static class AssertHelper
    {
        public static void AreEqual<T>(IScreenshotHelper? screenshotHelper, T expected, T actual, string message)
        {
            try
            {
                if (Assert.Equals(expected, actual))
                {
                    screenshotHelper?.CaptureAndAttach($"Assert Equals Passed: {message}", true);
                }
                else
                {
                    screenshotHelper?.CaptureAndAttach($"Assert Equals Failed: {message}", false);
                    Assert.Fail($"Assert.Equals failed: {message}");
                }
            }
            catch (Exception)
            {
                screenshotHelper?.CaptureAndAttach($"Assert Equals Failed: {message}", false);
                throw;
            }
        }

        public static void IsTrue(IScreenshotHelper? screenshotHelper, bool condition, string message)
        {
            try
            {
                if (condition)
                {
                    screenshotHelper?.CaptureAndAttach($"Assert True Passed: {message}", true);
                }
                else
                {
                    screenshotHelper?.CaptureAndAttach($"Assert True Failed: {message}", false);
                    Assert.Fail($"Assert.True failed: {message}");
                }
            }
            catch (Exception)
            {
                screenshotHelper?.CaptureAndAttach($"Assert True Failed: {message}", false);
                throw;
            }
        }

        // Add similar methods as needed, e.g., IsFalse, IsNull, etc.
    }
}
