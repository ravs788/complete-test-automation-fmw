using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Core.Utilities
{
    /// <summary>
    /// Lightweight connectivity probe for Elasticsearch. Intended to quickly determine if a server
    /// is reachable at the configured URL so the framework can disable logging when it's down.
    /// </summary>
    public static class ElasticConnectivity
    {
        /// <summary>
        /// Returns true if the elastic server at elasticUrl responds within the timeout.
        /// Treats any HTTP status (including 401/403) as "reachable" since the server is up.
        /// Returns false on network errors or timeouts.
        /// </summary>
        public static bool IsReachable(string? elasticUrl, string? username = null, string? password = null, TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(elasticUrl))
                return false;

            try
            {
                using var handler = new HttpClientHandler
                {
                    // Allow self-signed certs for local/dev scenarios. Secure profiles can harden this.
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                using var http = new HttpClient(handler)
                {
                    Timeout = timeout ?? TimeSpan.FromMilliseconds(750)
                };

                var req = new HttpRequestMessage(HttpMethod.Get, elasticUrl);

                if (!string.IsNullOrEmpty(username))
                {
                    var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password ?? string.Empty}"));
                    req.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
                }

                using var resp = http.Send(req);
                var code = (int)resp.StatusCode;
                // 2xx-4xx implies the server is up and responding; 5xx may still be up but erroring.
                return code >= 200 && code < 500;
            }
            catch
            {
                return false;
            }
        }
    }
}
