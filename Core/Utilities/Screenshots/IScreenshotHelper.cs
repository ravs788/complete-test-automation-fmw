namespace Core.Utilities
{
    /// <summary>
    /// Defines a simple contract for capturing and attaching screenshots.
    /// Implementation is platform-specific and should reside in the respective UI layer.
    /// </summary>
    public interface IScreenshotHelper
    {
        /// <summary>
        /// Captures and attaches a screenshot with metadata (step, status, etc.).
        /// </summary>
        /// <param name="stepDescription">A description for the screenshot step.</param>
        /// <param name="isSuccess">Indicates if the screenshot is for a passed or failed step.</param>
        void CaptureAndAttach(string stepDescription, bool isSuccess);
    }
}
