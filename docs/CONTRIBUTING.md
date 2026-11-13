# Contributing Guidelines

## Where things go

One rule everywhere: **`<Layer>/<App>/`** for app-specific code, the layer root for generic code, and **namespace = folder path**.

| You're adding… | Put it in |
|----------------|-----------|
| A page object | `Pages/<App>/` (inherit `Pages.Common.BasePage`) |
| A web UI test | `Tests/UI/Web/<App>/` (inherit `<App>TestBase`, which inherits `UiTestBase`) |
| An API test | `Tests/Api/` (inherit `ApiTestBase`) |
| A unit test for framework code (`Core`) | `Tests/Unit/<Area>/` (plain xUnit; no browser, no network) |
| A mobile page / test | `Tests/UI/Mobile/Pages/<App>/` (inherit `MobileBasePage`) / `Tests/UI/Mobile/<App>/` (inherit `MobileTestBase`) |
| A model (API contract or test-data shape) | `Core/Models/<App>/` |
| Test data for an entity | `Core/TestData/<App>/`: `<Entity>Builder` (valid by default, `With…` for variations) + `<Entity>Mother` (named common cases) |
| `[Theory]` cases | a `[MemberData]` source in the test class, built from Builders/Mothers (plain values, so each case is its own test) |
| Large scenario tables, captured payloads, data owned outside the code | `Tests/test-data/<App>/<TestClass>/<Scenario>.json`, loaded with `TestDataLoader` (the exception, not the default) |
| An API-specific helper (auth, …) | `Core/Api/<Service>/` |
| Settings | a `sealed record` + a cached `…Provider` (see `Core/Configuration`) |
| Other shared framework code (clients, logging, runner) | `Core/<Area>/` |

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
- A method that moves to another page returns that page (`Task<CartPage> OpenCartAsync()`). Tests only create entry pages (`new LoginPage(Page)`, `new TodoPage(Page)`) and get every other page from the step before it.
- Locate elements by test id (`TestId("…")`, from `BasePage`), role, label or placeholder (`GetByRole`, `GetByLabel`, `GetByPlaceholder`), never by CSS class. Set the app's test-id attribute once in its base page (`SauceDemoPage` uses `data-test`; the default is `data-testid`).
- Prefer Playwright's auto-retrying `Assertions.Expect(...)` over reading values and asserting on them; never add fixed sleeps.
- Wrap every test body in the base class's `RunAsync(...)` / `Run(...)` so it gets logging, Allure timing, failure screenshots and results publishing.
- Give each test class the same labels:
  - `[AllureParentSuite("API" | "UI Web" | "UI Mobile")]`, `[AllureSuite("<App>")]`, `[AllureSubSuite("<Feature>")]`
  - `[AllureTag(<layer>, [mobile,] <app>, <feature>, <categories>…)]`, e.g. `("ui", "saucedemo", "checkout", "regression")`
  - `[Trait("Category", ...)]`: `smoke` for the PR set, `regression` for everything
- Models are `sealed record`s with PascalCase properties and `[JsonPropertyName("…")]` for the wire name. Don't use lowercase C# properties or `Dictionary<string, …>` bodies.

## General

- Follow .NET naming conventions and `.editorconfig`: file-scoped namespaces, `using`s above the namespace with `System.*` first, nothing already covered by implicit usings (`System`, `System.Linq`, `System.Threading.Tasks`, `Xunit` in test projects, …), LF line endings (`.gitattributes`; `.bat` files keep CRLF).
- Run `dotnet format TestAutomationPlatform.slnx` before committing. The build reports style problems as warnings locally, and CI treats every warning as an error (`Directory.Build.props`). Set `CI=true` to build exactly like CI.
- One type per file; the file name matches the type.
- Test methods are named `Subject_Should_Outcome[_When_Condition]` in PascalCase segments, e.g. `Cart_Should_ContainProduct_When_ProductAdded`, `Ping_Should_Return201`. Renaming a test resets its Allure history, so choose the name carefully up front.
- Keep classes small and single-purpose (SOLID, Page Object Model).
- Run the relevant tests locally before opening a PR (`dotnet test Tests/Tests.csproj` or `scripts/*/run_*`).
