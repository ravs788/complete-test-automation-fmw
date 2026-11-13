# Contributing Guidelines

## Where things go

| You're adding… | Put it in |
|----------------|-----------|
| A page object | `Pages/<App>/` (inherit `Pages.Common.BasePage`) |
| A web UI test | `Tests/UI/Web/<App>/` (inherit `UiTestBase`) |
| An API test | `Tests/Api/` (inherit `ApiTestBase`) |
| A mobile test or page | `Tests/UI/Mobile/` (inherit `BaseMobileTest` / the mobile base pages) |
| Shared framework code (config, clients, logging, test-data models) | `Core/` |
| Test data | `Tests/test-data/<TestClass>/<Scenario>.json` |

Dependencies only point downwards: `Tests` → `Pages` → `Core`.

## Code Style

- **Always use `using` imports and never fully qualified names in the code.**
  - ❌ Do NOT: `await Microsoft.Playwright.Assertions.Expect(locator).ToBeVisibleAsync();`
  - ✅ Do:
    ```csharp
    using Microsoft.Playwright;
    ...
    await Assertions.Expect(locator).ToBeVisibleAsync();
    ```
- Page objects expose `ILocator`s for assertions and async intent methods (`LoginAsync`, `AddToCartAsync`) for actions. Keep assertions in tests, not in pages.
- Prefer Playwright's auto-retrying `Assertions.Expect(...)` over reading values and asserting on them; never add fixed sleeps.
- Wrap every test body in the base class's `RunAsync(...)` / `Run(...)` so it gets logging, Allure timing, failure screenshots and results publishing.
- Give each test class `[AllureSuite]`, `[AllureTag(...)]` and `[Trait("Category", ...)]` (`smoke` for the PR set, `regression` for everything).

## General

- Follow .NET naming conventions and `.editorconfig`.
- Organize and group `using` statements at the top of the file.
- Keep classes small and single-purpose (SOLID, Page Object Model).
- Run the relevant tests locally before opening a PR (`dotnet test Tests/Tests.csproj` or `scripts/*/run_*`).
