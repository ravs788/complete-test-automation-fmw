using Core.Ui.Selenium;

namespace Tests.Contract;

public sealed class SeleniumUiDriverContractTests(UiDriverFixture<SeleniumUiDriverFactory> fixture)
    : UiDriverContractTests<SeleniumUiDriverFactory>(fixture);
