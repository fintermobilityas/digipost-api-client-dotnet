using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Digipost.Api.Client.Api;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Entrypoint;
using Digipost.Api.Client.Common.Exceptions;
using Digipost.Api.Client.Common.Identify;
using Digipost.Api.Client.Common.Utilities;
using Digipost.Api.Client.Internal;
using Digipost.Api.Client.Resources.Certificate;
using Digipost.Api.Client.Resources.Xml;
using Digipost.Api.Client.Send;
using Digipost.Api.Client.Shared.Tests;
using Digipost.Api.Client.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Environment = Digipost.Api.Client.Common.Environment;

namespace Digipost.Api.Client.Tests.Integration;

public class DigipostApiIntegrationTests
{
    protected X509Certificate2 Certificate;
    protected ClientConfig ClientConfig;
    protected Uri Uri;

    public DigipostApiIntegrationTests()
    {
        ClientConfig = new ClientConfig(new Broker(1337), Environment.Production)
        {
            LogRequestAndResponse = false,
            TimeoutMilliseconds = 300000000
        };
        Uri = new Uri("/identification", UriKind.Relative);
        Certificate = CertificateResource.Certificate();
    }

    internal HttpClient GetHttpClient(
        FakeResponseHandler fakehandler)
    {
        ClientConfig.LogRequestAndResponse = true;
        var serviceProvider = LoggingUtility.CreateServiceProviderAndSetUpLogging();

        var allDelegationHandlers = new List<DelegatingHandler> { new AuthenticationHandler(ClientConfig, Certificate, serviceProvider.GetService<ILoggerFactory>()) };

        var httpClient = HttpClientFactory.Create(
            fakehandler,
            allDelegationHandlers.ToArray()
        );

        httpClient.BaseAddress = new Uri("http://www.fakeBaseAddress.no");

        return httpClient;
    }

    SendMessageApi GetDigipostApi(FakeResponseHandler fakeResponseHandler)
    {
        var httpClient = GetHttpClient(fakeResponseHandler);

        var serviceProvider = LoggingUtility.CreateServiceProviderAndSetUpLogging();

        var requestHelper = new RequestHelper(httpClient, serviceProvider.GetService<ILoggerFactory>()) { HttpClient = httpClient };

        var links = new Dictionary<string, Link>
        {
            ["SEARCH"] = new Link(httpClient.BaseAddress + $"{DomainUtility.GetSender().Id}/recipient/search") { Rel = httpClient.BaseAddress + "relations/search" },
            ["IDENTIFY_RECIPIENT"] = new Link(httpClient.BaseAddress + $"{DomainUtility.GetSender().Id}/identification") { Rel = httpClient.BaseAddress + "relations/identify_recipient" },
            ["CREATE_MESSAGE"] = new Link(httpClient.BaseAddress + $"{DomainUtility.GetSender().Id}/message") { Rel = httpClient.BaseAddress + "relations/create_message" }
        };
        var root = new Root("")
        {
            Links = links
        };

        var digipostApi = new SendMessageApi(new SendRequestHelper(requestHelper), serviceProvider.GetService<ILoggerFactory>(), root);
        return digipostApi;
    }

    public class SendMessageMethod : DigipostApiIntegrationTests
    {
        async Task SendMessageAsync(IMessage message, FakeResponseHandler fakeResponseHandler)
        {
            var digipostApi = GetDigipostApi(fakeResponseHandler);

            await digipostApi.SendMessageAsync(message);
        }

        [Fact]
        public async Task InternalServerErrorShouldCauseDigipostResponseException()
        {
            var message = DomainUtility.GetSimpleMessageWithRecipientById();
            const HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            var messageContent = new StringContent(string.Empty);

            await Assert.ThrowsAsync<ClientResponseException>(() =>
                SendMessageAsync(message, new FakeResponseHandler { ResultCode = statusCode, HttpContent = messageContent }));
        }

        [Fact]
        public async Task ProperRequestSentRecipientById()
        {
            var message = DomainUtility.GetSimpleMessageWithRecipientById();
            await SendMessageAsync(message, new FakeResponseHandler { ResultCode = HttpStatusCode.OK, HttpContent = XmlResource.SendMessage.GetMessageDelivery() });
        }

        [Fact]
        public async Task ProperRequestSentRecipientByNameAndAddress()
        {
            var message = DomainUtility.GetSimpleMessageWithRecipientByNameAndAddress();

            await SendMessageAsync(message, new FakeResponseHandler { ResultCode = HttpStatusCode.OK, HttpContent = XmlResource.SendMessage.GetMessageDelivery() });
        }

        [Fact]
        public async Task ShouldSerializeErrorMessage()
        {
            var message = DomainUtility.GetSimpleMessageWithRecipientById();
            const HttpStatusCode statusCode = HttpStatusCode.NotFound;
            var messageContent = XmlResource.SendMessage.GetError();

            await Assert.ThrowsAsync<ClientResponseException>(() => SendMessageAsync(message, new FakeResponseHandler { ResultCode = statusCode, HttpContent = messageContent }));
        }
    }

    public class SendIdentifyMethod : DigipostApiIntegrationTests
    {
        async Task IdentifyAsync(IIdentification identification)
        {
            var fakeResponseHandler = new FakeResponseHandler { ResultCode = HttpStatusCode.OK, HttpContent = XmlResource.Identification.GetResult() };
            var digipostApi = GetDigipostApi(fakeResponseHandler);

            await digipostApi.IdentifyAsync(identification);
        }

        [Fact]
        public async Task ProperRequestSent()
        {
            var identification = DomainUtility.GetPersonalIdentification();
            await IdentifyAsync(identification);
        }
    }
}