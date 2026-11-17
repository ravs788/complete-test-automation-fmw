namespace Core.Ui;

/// <summary>A UI assertion (<see cref="IElementAssertions"/>) did not hold within the timeout.</summary>
public sealed class UiAssertionException(string message, Exception? innerException = null) : UiException(message, innerException);
