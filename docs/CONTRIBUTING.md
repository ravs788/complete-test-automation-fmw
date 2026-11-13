# Contributing Guidelines for UI.Web

## Code Style

- **Always use `using` imports and never fully qualified domain names (FQDNs) in the code.**
  - ❌ Do NOT: `var driver = new OpenQA.Selenium.Firefox.FirefoxDriver();`
  - ✅ Do: 
    ```csharp
    using OpenQA.Selenium.Firefox;
    ...
    var driver = new FirefoxDriver();
    ```

This rule applies to all namespaces and types—keep top-level code clean and maintainable by relying on C# using statements.

## General

- Follow .NET naming conventions.
- Organize and group `using` statements at the top of the file.
- Keep all classes, pages, and utilities following SOLID and Page Object Model best practices.

---
