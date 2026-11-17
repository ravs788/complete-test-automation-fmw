namespace Core.Composition;

/// <summary>
/// Which provider implements each contract (R40). Loaded from <c>providers.json</c> by <see cref="ProvidersConfigProvider"/>;
/// env overrides use the <c>PROVIDERS_</c> prefix, e.g. <c>PROVIDERS_Logging=console</c> or
/// <c>PROVIDERS_Secrets__1=azure-keyvault</c>.
/// </summary>
public sealed record ProvidersSettings
{
    /// <summary>Logging provider name, e.g. <c>console</c> or <c>elastic</c>. Unavailable providers fall back to console.</summary>
    public string Logging { get; init; } = "console";

    /// <summary>Results publisher name; empty means "same as <see cref="Logging"/>".</summary>
    public string ResultsPublisher { get; init; } = "";

    /// <summary>Test reporter name, e.g. <c>allure</c> or <c>console</c>. Unregistered names fall back to console.</summary>
    public string Reporting { get; init; } = "console";

    /// <summary>
    /// Secret providers, tried in order for each <c>secret://NAME</c> placeholder, e.g. <c>["environment", "azure-keyvault"]</c>.
    /// Empty means <c>["environment"]</c>.
    /// </summary>
    public string[] Secrets { get; init; } = [];

    /// <summary>The results publisher to use: <see cref="ResultsPublisher"/>, or <see cref="Logging"/> when it is empty.</summary>
    public string EffectiveResultsPublisher => string.IsNullOrWhiteSpace(ResultsPublisher) ? Logging : ResultsPublisher;

    /// <summary>The secret providers to use, with the default applied.</summary>
    public IReadOnlyList<string> EffectiveSecrets => Secrets is { Length: > 0 } ? Secrets : ["environment"];
}
