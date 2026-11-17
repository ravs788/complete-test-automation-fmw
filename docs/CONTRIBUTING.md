# Contributing Guidelines

## Where things go

One rule everywhere: **`<Layer>/<App>/`** for app-specific code, the layer root for generic code, and **namespace = folder path**.

| You're adding… | Put it in |
|----------------|-----------|
| A page object | `Pages/<App>/` (inherit `Pages.Common.BasePage`) |
| A web UI test | `Tests/UI/Web/<App>/` (inherit `<App>TestBase`, which inherits `UiTestBase`) |
| An API test | `Tests/Api/` (inherit `ApiTestBase`) |
| A unit test for framework code (`Core`) | `Tests/Unit/<Area>/` (plain xUnit; no browser or external services; isolated loopback fixtures are allowed for transport checks) |
| A mobile page / test | the shared `Pages/<App>/` (same page objects as web) / `Tests/UI/Mobile/<App>/` (inherit `MobileTestBase`, `RunAsync`) |
| Reusable mobile infrastructure (settings, driver setup, interaction helpers) | `Core/Mobile/<Area>/` (project `Core.Mobile`; no page objects or tests) |
| A model (API contract or test-data shape) | `Domain/Models/<App>/` |
| Test data for an entity | `Domain/TestData/<App>/`: `<Entity>Builder` (valid by default, `With…` for variations) + `<Entity>Mother` (named common cases) |
| `[Theory]` cases | a `[MemberData]` source in the test class, built from Builders/Mothers (plain values, so each case is its own test) |
| Large scenario tables, captured payloads, data owned outside the code | `Tests/test-data/<App>/<TestClass>/<Scenario>.json`, loaded with `TestDataLoader` (the exception, not the default) |
| An API-specific helper (auth, …) | `Domain/Api/<Service>/` |
| Settings | a `sealed record` + a cached `…Provider` (see `Core/Configuration`); secrets as `secret://NAME`, never literal values |
| Database access in a test | `Core.Data.DbConnectionFactory` + `DbTestScope` (rolls back by default); the provider package goes in the test project, never in Core |
| Anything a test creates outside a transaction | register its removal with `Cleanup.Add(...)` (`Core.Testing.CleanupTracker`) |
| Waiting for a broker message | `RabbitMqListener` / `KafkaListener` (add-ons) started before the action, then `WaitForAsync(match, timeout)`; never sleep-and-read |
| A test that needs external infrastructure (broker, vault) | `[RequiresEnvironmentFact("VARIABLE")]` + `[Trait("Category", "integration")]`, so it is skipped unless the variable is set |
| An optional add-on that brings a heavy dependency (device driver, vault SDK, broker client) | its own project inside `Core/`, next to the area it extends (`Core/<Area>/<Name>/`, assembly `Core.<Area>.<Name>`, referencing `Core.csproj`); add the folder to `DefaultItemExcludes` in `Core/Core.csproj`, to `ci.slnf`, and to the `/Core/...` folder in the `.slnx` |
| Other shared framework code (clients, logging, runner) | `Core/<Area>/`: domain-agnostic only. Anything that names an app goes in `Domain/` (enforced by `CoreBoundaryTests`). |

Dependencies only point downwards: `Tests` → `Pages` → `Core`.

When a change alters a dependency, a step in a test run, how settings load or the CI flow, update the matching Mermaid diagram in [DIAGRAMS.md](DIAGRAMS.md) in the same PR (preview it in the [Mermaid Live Editor](https://mermaid.live/) or a Markdown preview with Mermaid support).

## Adding or replacing a library

Every external library belongs behind a framework contract, in its own adapter project under `Core/<Area>/<Vendor>/`. Before adding a package, check the inventory and rules in [PLUGIN_ARCHITECTURE.md](PLUGIN_ARCHITECTURE.md); to add a provider for an existing contract, follow its [Writing a provider](PLUGIN_ARCHITECTURE.md#5-writing-a-provider) steps (project layout, registration, the guarantees each kind of provider must keep, contract tests, docs). Don't import a vendor namespace in `Tests`, `Pages` or `Domain`, and don't add a vendor package to `Core/Core.csproj`.

## Code Style

- **Always use `using` imports and never fully qualified names in the code.**
  - ❌ Do NOT: `await Core.Ui.Ui.Expect(element).ToBeVisibleAsync();`
  - ✅ Do:
    ```csharp
    using Core.Ui;
    ...
    await Ui.Expect(element).ToBeVisibleAsync();
    ```
- Page objects work on the UI contract (`Core.Ui`: `IUiSession`, `IElement`, `Locate`) and never import a driver's namespace. They expose `IElement`s for assertions and async intent methods (`LoginAsync`, `AddToCartAsync`) for actions. Keep assertions in tests, not in pages.
- A method that moves to another page returns that page (`Task<CartPage> OpenCartAsync()`). Tests only create entry pages (`new LoginPage(Session)`, `new TodoPage(Session)`) and get every other page from the step before it.
- Locate elements by test id (`TestId("…")` / `ByTestId("…")`, from `BasePage`), role or placeholder (`Locate.Role(UiRole.Button, "Remove")`, `Locate.Placeholder("…")`), and narrow lists with `WithChild(ByTestId("…"), text)`, never by CSS class. If the contract lacks something you need, add it to `Core.Ui` (and to every driver adapter) rather than reaching for `Session.Native<T>()`. Set the app's test-id attribute once in its base page (`SauceDemoPage` uses `data-test`; the default is `data-testid`).
- Prefer the auto-retrying `Ui.Expect(...)` assertions over reading values and asserting on them; never add fixed sleeps.
- For transient failures use `Core.Resilience`: `RetryPolicies` (transient HTTP for idempotent requests only, navigation, UI actions; implemented by the `"Resilience"` provider in `providers.json`, `polly` or `builtin`) and `Poll.UntilAsync(condition, timeout)` for eventually consistent backends. Don't write retry loops or `Thread.Sleep`, and never retry assertions.
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
