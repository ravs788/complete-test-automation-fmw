namespace Core.Ui;

/// <summary>A UI action or lookup failed (element missing, not actionable, navigation error, …), whatever the driver (R42).</summary>
public class UiException(string message, Exception? innerException = null) : Exception(message, innerException);
