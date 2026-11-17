using Core.Ui.Playwright;

namespace Tests.Contract;

public sealed class PlaywrightUiDriverContractTests(UiDriverFixture<PlaywrightUiDriverFactory> fixture)
    : UiDriverContractTests<PlaywrightUiDriverFactory>(fixture);
