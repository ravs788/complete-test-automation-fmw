using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace UI.Mobile.Utilities
{
    /// <summary>
    /// Gesture helpers using Appium mobile commands (Appium v2, W3C actions).
    /// Works with both Android (UiAutomator2) and iOS (XCUITest) where supported.
    /// </summary>
    public static class GestureHelper
    {
        /// <summary>
        /// Tap the center of an element found by locator.
        /// </summary>
        public static void Tap(AppiumDriver driver, By locator)
        {
            var el = driver.FindElement(locator);
            Tap(driver, el);
        }

        /// <summary>
        /// Tap the center of a given element.
        /// </summary>
        public static void Tap(AppiumDriver driver, IWebElement element)
        {
            var center = ElementCenter(element);
            var args = new Dictionary<string, object>
            {
                ["x"] = center.x,
                ["y"] = center.y
            };

            TryExecute(() => driver.ExecuteScript("mobile: clickGesture", args));
        }

        /// <summary>
        /// Long press the center of an element for durationMs.
        /// </summary>
        public static void LongPress(AppiumDriver driver, By locator, int durationMs = 800)
        {
            var el = driver.FindElement(locator);
            LongPress(driver, el, durationMs);
        }

        public static void LongPress(AppiumDriver driver, IWebElement element, int durationMs = 800)
        {
            var center = ElementCenter(element);
            var args = new Dictionary<string, object>
            {
                ["x"] = center.x,
                ["y"] = center.y,
                ["duration"] = durationMs
            };

            TryExecute(() => driver.ExecuteScript("mobile: longClickGesture", args));
        }

        /// <summary>
        /// Swipe on screen in the given direction using a viewport-based region.
        /// direction: "up" | "down" | "left" | "right"
        /// percent: 0..1 of swipe distance relative to region
        /// </summary>
        public static void SwipeOnScreen(AppiumDriver driver, string direction, double percent = 0.75)
        {
            var size = driver.Manage().Window.Size;
            var args = new Dictionary<string, object>
            {
                ["left"] = 0,
                ["top"] = 0,
                ["width"] = size.Width,
                ["height"] = size.Height,
                ["direction"] = NormalizeDirection(direction),
                ["percent"] = ClampPercent(percent)
            };

            TryExecute(() => driver.ExecuteScript("mobile: swipeGesture", args));
        }

        /// <summary>
        /// Swipe within an element in the given direction.
        /// </summary>
        public static void SwipeInElement(AppiumDriver driver, IWebElement element, string direction, double percent = 0.75)
        {
            var loc = element.Location;
            var size = element.Size;
            var args = new Dictionary<string, object>
            {
                ["left"] = loc.X,
                ["top"] = loc.Y,
                ["width"] = size.Width,
                ["height"] = size.Height,
                ["direction"] = NormalizeDirection(direction),
                ["percent"] = ClampPercent(percent)
            };

            TryExecute(() => driver.ExecuteScript("mobile: swipeGesture", args));
        }

        /// <summary>
        /// Scroll the screen in a direction (best-effort). On Android/iOS, Appium v2 supports mobile: scrollGesture.
        /// </summary>
        public static void ScrollOnScreen(AppiumDriver driver, string direction, double percent = 0.75)
        {
            var size = driver.Manage().Window.Size;
            var args = new Dictionary<string, object>
            {
                ["left"] = 0,
                ["top"] = 0,
                ["width"] = size.Width,
                ["height"] = size.Height,
                ["direction"] = NormalizeDirection(direction),
                ["percent"] = ClampPercent(percent)
            };

            TryExecute(() => driver.ExecuteScript("mobile: scrollGesture", args));
        }

        /// <summary>
        /// Attempts to scroll until the element is found or maxScrolls is reached.
        /// Uses repeated scroll gestures in the given direction.
        /// </summary>
        public static bool ScrollUntilVisible(AppiumDriver driver, By locator, string direction = "down", int maxScrolls = 5)
        {
            for (int i = 0; i < maxScrolls; i++)
            {
                if (Exists(driver, locator))
                    return true;

                ScrollOnScreen(driver, direction, 0.75);
            }

            return Exists(driver, locator);
        }

        /// <summary>
        /// Checks if an element exists (without throwing) using a short implicit poll.
        /// </summary>
        public static bool Exists(AppiumDriver driver, By locator)
        {
            try
            {
                driver.FindElement(locator);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Helpers

        private static (int x, int y) ElementCenter(IWebElement element)
        {
            var loc = element.Location;
            var size = element.Size;
            var x = loc.X + size.Width / 2;
            var y = loc.Y + size.Height / 2;
            return (x, y);
        }

        private static string NormalizeDirection(string direction)
        {
            direction = (direction ?? "").Trim().ToLowerInvariant();
            return direction switch
            {
                "up" => "up",
                "down" => "down",
                "left" => "left",
                "right" => "right",
                _ => "down"
            };
        }

        private static double ClampPercent(double percent)
        {
            if (double.IsNaN(percent) || percent <= 0) return 0.01;
            if (percent > 1.0) return 1.0;
            return percent;
        }

        private static void TryExecute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                // Swallow to make gestures best-effort and not fatal to flows.
                // Callers can decide if failure is critical.
            }
        }
    }
}
