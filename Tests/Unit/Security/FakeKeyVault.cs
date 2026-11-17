using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;

namespace Tests.Unit.Security;

/// <summary>A Key Vault client double: SecretClient is designed to be mocked through its protected constructor.</summary>
internal sealed class FakeSecretClient(Dictionary<string, string> secrets, int failWithStatus = 0) : SecretClient
{
    public int Calls { get; private set; }
    public override Uri VaultUri => new("https://unit-test.vault.azure.net/");

    public override Response<KeyVaultSecret> GetSecret(string name, string? version = null, CancellationToken cancellationToken = default)
    {
        Calls++;
        if (failWithStatus != 0) throw new RequestFailedException(failWithStatus, "access denied");
        return secrets.TryGetValue(name, out var value)
            ? Response.FromValue(SecretModelFactory.KeyVaultSecret(new SecretProperties(name), value), new FakeResponse())
            : throw new RequestFailedException(404, "SecretNotFound");
    }
}

internal sealed class FakeResponse : Response
{
    public override int Status => 200;
    public override string ReasonPhrase => "OK";
    public override Stream? ContentStream { get; set; }
    public override string ClientRequestId { get; set; } = "";
    public override void Dispose() { }
    protected override bool ContainsHeader(string name) => false;
    protected override IEnumerable<HttpHeader> EnumerateHeaders() => [];
    protected override bool TryGetHeader(string name, [NotNullWhen(true)] out string? value) { value = null; return false; }
    protected override bool TryGetHeaderValues(string name, [NotNullWhen(true)] out IEnumerable<string>? values) { values = null; return false; }
}
