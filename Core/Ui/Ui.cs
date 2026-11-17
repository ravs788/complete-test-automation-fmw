namespace Core.Ui;

/// <summary>Entry point for UI assertions in tests (R42): <c>await Ui.Expect(page.Title).ToHaveTextAsync("Products")</c>.</summary>
public static class Ui
{
    public static IElementAssertions Expect(IElement element) => element.Expect();
}
