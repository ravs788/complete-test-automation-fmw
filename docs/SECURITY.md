# Security controls for test runs

This framework is a test client. Configure its endpoints and diagnostics for the data and environments a suite uses. The controls below do not establish application-under-test security or organizational compliance.

## Local logging and remote endpoints

The development Compose stack disables Elasticsearch authentication and binds Elasticsearch/Kibana to `127.0.0.1` only. Use a separately secured deployment for shared access. Do not broaden those bindings as a substitute for authentication.

Remote Elasticsearch endpoints require HTTPS. HTTP is supported only for loopback development. Supply credentials through the existing secret settings/environment providers; URL user information is rejected. Normal TLS validation and configured certificate fingerprints remain in effect, with the existing explicit loopback development exception.

## Redirects and diagnostic redaction

Both HTTP transports return the first 3xx response without following it. This applies even to same-origin redirects: callers that need redirects must make an explicit destination and method/body decision. Authentication headers and request bodies therefore cannot be automatically replayed to a redirect destination by these transports.

Framework diagnostic boundaries redact URL user information, query values, fragments, common credential/header fields and secrets registered by settings loading. Console/Elastic messages and metadata, API errors, provider fallback reasons and report-step failures use the same redactor. Secret-bearing test failures are replaced with a sanitized exception containing safe original type/stack text and no raw inner exception. Requests and responses used by assertions are not rewritten.

Redaction cannot identify arbitrary private business data, secrets embedded in an unknown format, or data independently written by a test/vendor outside these boundaries. It does not scrub binary screenshots/traces or previously saved artifacts. Avoid real secrets in xUnit theory arguments, test names or custom labels, because the native host can serialize them independently; resolve credentials inside fixtures/settings instead.

## Sensitive captures and web CI publication

Set `DIAGNOSTIC_ARTIFACTS=restricted` to disable the shared web test base's automatic screenshots and tracing. The built-in console/Allure reporters also suppress explicit attachment publication in this mode. `standard` (the demo-suite default) retains the existing capture behavior. Unknown modes fail configuration rather than enable captures. Test-owned browser/session cleanup and test outcomes are unchanged.

For example, a native web run under an authorized execution exception can use:

```bash
DIAGNOSTIC_ARTIFACTS=restricted BROWSER=chrome HEADLESS=false \
  dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Tests.UI.Web."
```

Azure manual runs expose two parameters for the Linux web/API job:

| Parameter | Default | Sensitive-run choice |
|---|---|---|
| `diagnosticArtifactMode` | `standard` | `restricted`: disable automatic web captures and built-in reporter attachments |
| `publishWebDiagnostics` | `true` | `false`: suppress web TRX/coverage publication and Allure artifact upload/history/generation |

Normal test execution and its exit code still gate the job when publication is disabled. These parameters do not remove Azure build logs or change server-side access/retention. Configure and verify those controls in Azure for the applicable project, identities and data; this repository cannot attest to their effective settings. For sensitive push/scheduled suites, set appropriate YAML defaults rather than relying on manual-run selection.

Mobile and Windows helper publication retain their existing job configuration. Mobile work remains deferred (R16/R32/R33); the new web-job parameters do not claim to harden or verify mobile captures. Direct driver use or third-party reporters outside the shared web base/built-in reporters must enforce the capture policy themselves.

Before using non-demo sensitive data, verify capture suppression with synthetic data and confirm effective artifact/log reader permissions and retention. Keep raw traces restricted: they may include DOM snapshots, source, and network headers/bodies. Do not attempt to sanitize trace ZIPs by string replacement.

## Verification evidence

See [REVIEW.md](REVIEW.md) R61–R65 for code checks, isolated synthetic-credential tests and outstanding environment verification. Never put real credentials or their raw diagnostic output in the ledger.
