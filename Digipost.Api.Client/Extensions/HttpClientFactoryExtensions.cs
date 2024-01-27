using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Digipost.Api.Client.Extensions
{
    public static class HttpClientFactoryExtensions
    {
        /// <summary>
        /// Adds a Digipost client to the service collection using the specified client configuration and certificate.
        /// </summary>
        /// <param name="serviceCollection">The IServiceCollection in which the Digipost client will be added.</param>
        /// <param name="clientConfig">The client configuration containing the necessary settings for the Digipost client.</param>
        /// <param name="certificate">The certificate used for authentication.</param>
        /// <returns>An IHttpClientBuilder instance that can be used to further configure the Digipost client.</returns>
        public static IHttpClientBuilder AddDigipostClient(
            this IServiceCollection serviceCollection, 
            ClientConfig clientConfig, 
            X509Certificate2 certificate)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            if (clientConfig == null) throw new ArgumentNullException(nameof(clientConfig));

            var httpClientName = $"Digipost:{clientConfig.Broker.Id}";

            serviceCollection.AddTransient(c => new AuthenticationHandler(clientConfig, certificate, c.GetRequiredService<ILoggerFactory>()));
            serviceCollection.AddTransient<IDigipostClient>(c => new DigipostClient(clientConfig, 
                c.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientName), c.GetRequiredService<ILoggerFactory>()));

            return serviceCollection.AddHttpClient(
                    httpClientName, client =>
                    {
                        client.Timeout = TimeSpan.FromMilliseconds(clientConfig.TimeoutMilliseconds);
                        client.BaseAddress = new Uri(clientConfig.Environment.Url.AbsoluteUri);
                    })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var httpMessageHandler = new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    };

                    if (clientConfig.WebProxy != null)
                    {
                        httpMessageHandler.Proxy = clientConfig.WebProxy;
                        httpMessageHandler.UseProxy = true;
                        httpMessageHandler.UseDefaultCredentials = false;
                    }

                    return httpMessageHandler;
                })
                .AddHttpMessageHandler<AuthenticationHandler>();
        }
    }
}
