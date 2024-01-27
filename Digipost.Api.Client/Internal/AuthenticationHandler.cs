using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Digipost.Api.Client.Common;
using Microsoft.Extensions.Logging;
using Environment = System.Environment;

namespace Digipost.Api.Client.Internal;

internal class AuthenticationHandler : DelegatingHandler
{
    static ILogger<DigipostClient> _logger;
    static readonly string UserAgent = $"digipost-api-client-dotnet/{Assembly.GetExecutingAssembly().GetName().Version} (netcore/{Environment.Version})";

    public AuthenticationHandler(ClientConfig clientConfig, X509Certificate2 businessCertificate, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<DigipostClient>();
        ClientConfig = clientConfig;
        BusinessCertificate = businessCertificate;
        Method = WebRequestMethods.Http.Get;
    }

    ClientConfig ClientConfig { get; }

    X509Certificate2 BusinessCertificate { get; }

    string Method { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var date = DateTime.UtcNow.ToString("R");
        var brokerId = ClientConfig.Broker.Id.ToString();

        request.Headers.Add("X-Digipost-UserId", brokerId);
        request.Headers.Add("Date", date);
        request.Headers.Add("Accept", DigipostVersion.V8);
        request.Headers.Add("User-Agent", UserAgent);
        Method = request.Method.ToString();

        string contentHash = null;

        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            contentHash = ComputeHash(contentBytes);
            request.Headers.Add("X-Content-SHA256", contentHash);
        }

        var signature = ComputeSignature(Method, request.RequestUri, date, contentHash, brokerId, BusinessCertificate, ClientConfig.LogRequestAndResponse);
        request.Headers.Add("X-Digipost-Signature", signature);

        return await base.SendAsync(request, cancellationToken);
    }

    static string GetAssemblyVersion()
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(inputBytes);
        return Convert.ToBase64String(hashBytes);
    }

    internal static string ComputeSignature(string method, Uri uri, string date, string contentSha256Hash,
        string userId, X509Certificate2 businessCertificate, bool logRequestAndResponse)
    {
        var uriParts = new UriParts(uri);

        if (logRequestAndResponse)
        {
            _logger.LogDebug("Compute signature, canonical string generated by .NET Client:");
            _logger.LogDebug("=== SIGNATURE DATA START===");
        }

        string messageHeader;

        if (contentSha256Hash != null)
        {
            messageHeader = method.ToUpper() + "\n" +
                            uriParts.AbsoluteUri + "\n" +
                            "date: " + date + "\n" +
                            "x-content-sha256: " + contentSha256Hash + "\n" +
                            "x-digipost-userid: " + userId + "\n" +
                            uriParts.Parameters.ToLower() + "\n";
        }
        else
        {
            messageHeader = method.ToUpper() + "\n" +
                            uriParts.AbsoluteUri + "\n" +
                            "date: " + date + "\n" +
                            "x-digipost-userid: " + userId + "\n" +
                            uriParts.Parameters.ToLower() + "\n";
        }

        if (logRequestAndResponse)
        {
            _logger.LogDebug(messageHeader);
            _logger.LogDebug("=== SIGNATURE DATA END ===");
        }

        var messageBytes = Encoding.UTF8.GetBytes(messageHeader);

        using var rsa = businessCertificate.GetRSAPrivateKey();
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(messageBytes);
        var signature = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signature);
    }

    sealed class UriParts
    {
        public UriParts(Uri uri)
        {
            var datUri = uri.IsAbsoluteUri ? uri.AbsolutePath : $"/{uri.OriginalString}";
            AbsoluteUri = datUri.ToLower();
            Parameters = uri.Query.Length > 0 ? uri.Query.Substring(1) : "";
        }

        public string AbsoluteUri { get; }

        public string Parameters { get; }
    }
}