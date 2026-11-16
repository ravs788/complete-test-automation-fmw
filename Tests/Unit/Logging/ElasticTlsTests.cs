using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Core.Logging.Elastic;

namespace Tests.Unit.Logging;

public class ElasticTlsTests
{
    [Theory]
    [InlineData(ElasticServerChoices.ON_CLOUD, false, false)]
    [InlineData(ElasticServerChoices.ON_LOCALHOST_SECURE, true, true)]
    [InlineData(ElasticServerChoices.ON_LOCALHOST_SECURE, false, false)]
    public async Task Check_Should_SendCredentials_OnlyAfterCertificateIsAccepted(
        ElasticServerChoices profile, bool matchingFingerprint, bool expectedUsable)
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));
        using var otherKey = RSA.Create(2048);
        var otherRequest = new CertificateRequest("CN=other", otherKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var otherCertificate = otherRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));
        var fingerprint = (matchingFingerprint ? certificate : otherCertificate).GetCertHashString(HashAlgorithmName.SHA256);

        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var receivedHeaders = string.Empty;
        var server = Task.Run(async () =>
        {
            using var socket = await listener.AcceptTcpClientAsync(cancellation.Token);
            using var tls = new SslStream(socket.GetStream());
            try
            {
                await tls.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                {
                    ServerCertificate = certificate,
                    EnabledSslProtocols = SslProtocols.Tls12
                }, cancellation.Token);
                var buffer = new byte[1];
                var headers = new StringBuilder();
                while (!headers.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
                {
                    if (await tls.ReadAsync(buffer, cancellation.Token) == 0)
                    {
                        return;
                    }
                    headers.Append((char)buffer[0]);
                }
                receivedHeaders = headers.ToString();
                const string body = "{\"version\":{\"number\":\"8.15.3\"}}";
                var response = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nX-Elastic-Product: Elasticsearch\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n{body}");
                await tls.WriteAsync(response, cancellation.Token);
            }
            catch (AuthenticationException)
            {
                // A rejected server certificate may terminate the handshake.
            }
            catch (IOException)
            {
                // The client can also reject the certificate immediately after the handshake.
            }
        }, cancellation.Token);

        var result = await Task.Run(() => ElasticConnectivity.Check($"https://localhost:{port}", "probe-user", "probe-password",
            TimeSpan.FromSeconds(3), profile, fingerprint), cancellation.Token);
        await server.WaitAsync(cancellation.Token);

        Assert.Equal(expectedUsable, result.IsUsable);
        if (expectedUsable)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("probe-user:probe-password"));
            Assert.Contains($"Authorization: Basic {credentials}", receivedHeaders, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.Empty(receivedHeaders);
        }
    }
}
