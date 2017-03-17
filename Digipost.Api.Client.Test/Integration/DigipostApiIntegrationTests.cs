using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Digipost.Api.Client.Api;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Actions;
using Digipost.Api.Client.Common.Handlers;
using Digipost.Api.Client.Common.Utilities;
using Digipost.Api.Client.Domain.Exceptions;
using Digipost.Api.Client.Domain.Identify;
using Digipost.Api.Client.Domain.SendMessage;
using Digipost.Api.Client.Resources.Certificate;
using Digipost.Api.Client.Resources.Xml;
using Digipost.Api.Client.Test.Fakes;
using Moq;
using Xunit;
using Environment = Digipost.Api.Client.Common.Environment;

namespace Digipost.Api.Client.Test.Integration
{
    public class DigipostApiIntegrationTests
    {
        protected X509Certificate2 Certificate;
        protected ClientConfig ClientConfig;
        protected Uri Uri;

        public DigipostApiIntegrationTests()
        {
            ClientConfig = new ClientConfig("1337", Environment.Production)
            {
                LogRequestAndResponse = false,
                TimeoutMilliseconds = 300000000
            };
            Uri = new Uri("/identification", UriKind.Relative);
            Certificate = CertificateResource.Certificate();
        }

        internal AuthenticationHandler CreateHandlerChain(
            FakeResponseHandler fakehandler)
        {
            var loggingHandler = new LoggingHandler(fakehandler, ClientConfig);
            var authenticationHandler = new AuthenticationHandler(ClientConfig, Certificate, loggingHandler);
            return authenticationHandler;
        }

        public class SendMessageMethod : DigipostApiIntegrationTests
        {
            private void SendMessage(IMessage message, FakeResponseHandler fakeResponseHandler)
            {
                var fakehandler = fakeResponseHandler;
                var fakeHandlerChain = CreateHandlerChain(fakehandler);
                var mockFacktory = CreateMockFactoryReturningMessage(message, fakeHandlerChain);

                var requestHelper = new RequestHelper(ClientConfig, Certificate) {DigipostActionFactory = mockFacktory.Object};
                var digipostApi = new DigipostApi(ClientConfig, Certificate, requestHelper);

                digipostApi.SendMessage(message);
            }

            private Mock<DigipostActionFactory> CreateMockFactoryReturningMessage(IMessage message, AuthenticationHandler authenticationHandler)
            {
                var mockFacktory = new Mock<DigipostActionFactory>();
                mockFacktory.Setup(
                        f =>
                            f.CreateClass(message, It.IsAny<ClientConfig>(), It.IsAny<X509Certificate2>(),
                                It.IsAny<Uri>()))
                    .Returns(new MessageAction(message, ClientConfig, Certificate, Uri)
                    {
                        HttpClient = new HttpClient(authenticationHandler) {BaseAddress = new Uri("http://tull")}
                    });
                return mockFacktory;
            }

            [Fact]
            public void InternalServerErrorShouldCauseDigipostResponseException()
            {
                try
                {
                    var message = DomainUtility.GetSimpleMessageWithRecipientById();
                    const HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
                    var messageContent = new StringContent(string.Empty);

                    SendMessage(message, new FakeResponseHandler {ResultCode = statusCode, HttpContent = messageContent});
                }
                catch (AggregateException e)
                {
                    var ex = e.InnerExceptions.ElementAt(0);
                    Assert.True(ex.GetType() == typeof(ClientResponseException));
                }
            }

            [Fact]
            public void ProperRequestSentRecipientById()
            {
                var message = DomainUtility.GetSimpleMessageWithRecipientById();
                SendMessage(message, new FakeResponseHandler {ResultCode = HttpStatusCode.OK, HttpContent = XmlResource.SendMessage.GetMessageDelivery()});
            }

            [Fact]
            public void ProperRequestSentRecipientByNameAndAddress()
            {
                var message = DomainUtility.GetSimpleMessageWithRecipientByNameAndAddress();

                SendMessage(message, new FakeResponseHandler {ResultCode = HttpStatusCode.OK, HttpContent = XmlResource.SendMessage.GetMessageDelivery()});
            }

            [Fact]
            public void ShouldSerializeErrorMessage()
            {
                try
                {
                    var message = DomainUtility.GetSimpleMessageWithRecipientById();
                    const HttpStatusCode statusCode = HttpStatusCode.NotFound;
                    var messageContent = XmlResource.SendMessage.GetError();

                    SendMessage(message, new FakeResponseHandler {ResultCode = statusCode, HttpContent = messageContent});
                }
                catch (AggregateException e)
                {
                    var ex = e.InnerExceptions.ElementAt(0);

                    Assert.True(ex.GetType() == typeof(ClientResponseException));
                }
            }
        }

        public class SendIdentifyMethod : DigipostApiIntegrationTests
        {
            private void Identify(IIdentification identification)
            {
                var fakehandler = new FakeResponseHandler {ResultCode = HttpStatusCode.OK, HttpContent = XmlResource.Identification.GetResult()};
                var fakeHandlerChain = CreateHandlerChain(fakehandler);
                var mockFactory = CreateMockFactoryReturningIdentification(identification, fakeHandlerChain);

                var requestHelper = new RequestHelper(ClientConfig, Certificate) {DigipostActionFactory = mockFactory.Object};
                var digipostApi = new DigipostApi(ClientConfig, Certificate, requestHelper);

                digipostApi.Identify(identification);
            }

            private Mock<DigipostActionFactory> CreateMockFactoryReturningIdentification(IIdentification identification, AuthenticationHandler authenticationHandler)
            {
                var mockFactory = new Mock<DigipostActionFactory>();
                mockFactory.Setup(
                        f =>
                            f.CreateClass(identification, It.IsAny<ClientConfig>(), It.IsAny<X509Certificate2>(),
                                It.IsAny<Uri>()))
                    .Returns(new IdentificationAction(identification, ClientConfig, Certificate, Uri)
                    {
                        HttpClient =
                            new HttpClient(authenticationHandler) {BaseAddress = new Uri("http://tull")}
                    });
                return mockFactory;
            }

            [Fact]
            public void ProperRequestSent()
            {
                var identification = DomainUtility.GetPersonalIdentification();
                Identify(identification);
            }

            [Fact]
            public void ProperRequestWithIdSent()
            {
                var identification = DomainUtility.GetPersonalIdentificationById();
                Identify(identification);
            }

            [Fact]
            public void ProperRequestWithNameAndAddressSent()
            {
                var identification = DomainUtility.GetPersonalIdentificationByNameAndAddress();
                Identify(identification);
            }
        }

        public class SearchMethod : DigipostApiIntegrationTests
        {
            private Mock<DigipostActionFactory> CreateMockFactoryReturningSearch(AuthenticationHandler fakeHandlerChain)
            {
                var mockFacktory = new Mock<DigipostActionFactory>();
                mockFacktory.Setup(
                        f =>
                            f.CreateClass(It.IsAny<ClientConfig>(), It.IsAny<X509Certificate2>(),
                                It.IsAny<Uri>()))
                    .Returns(new UriAction(null, ClientConfig, Certificate, Uri)
                    {
                        HttpClient =
                            new HttpClient(fakeHandlerChain) {BaseAddress = new Uri("http://tull")}
                    });
                return mockFacktory;
            }

            [Fact]
            public void ProperRequestSent()
            {
                const string searchString = "jarand";

                var fakehandler = new FakeResponseHandler {ResultCode = HttpStatusCode.OK, HttpContent = XmlResource.Search.GetResult()};
                var fakeHandlerChain = CreateHandlerChain(fakehandler);
                var mockFactory = CreateMockFactoryReturningSearch(fakeHandlerChain);

                var requestHelper = new RequestHelper(ClientConfig, Certificate) {DigipostActionFactory = mockFactory.Object};
                var digipostApi = new DigipostApi(ClientConfig, Certificate, requestHelper);

                var result = digipostApi.Search(searchString);
                Assert.NotNull(result);
            }
        }

        public class GetStreamMethod : DigipostApiIntegrationTests
        {
            private Mock<DigipostActionFactory> CreateMockFactoryReturningSearch(AuthenticationHandler fakeHandlerChain)
            {
                var mockFacktory = new Mock<DigipostActionFactory>();
                mockFacktory.Setup(
                        f =>
                            f.CreateClass(It.IsAny<ClientConfig>(), It.IsAny<X509Certificate2>(),
                                It.IsAny<Uri>()))
                    .Returns(new UriAction(null, ClientConfig, Certificate, Uri)
                    {
                        HttpClient =
                            new HttpClient(fakeHandlerChain) { BaseAddress = new Uri("http://tull") }
                    });
                return mockFacktory;
            }

            [Fact]
            public void ProperRequestSent() //Todo: Fix test when factory mocking is removed
            {
                const string searchString = "jarand";

                var fakehandler = new FakeResponseHandler { ResultCode = HttpStatusCode.NotFound, HttpContent = XmlResource.Inbox.GetError()};
                var fakeHandlerChain = CreateHandlerChain(fakehandler);
                var mockFactory = CreateMockFactoryReturningSearch(fakeHandlerChain);

                var requestHelper = new RequestHelper(ClientConfig, Certificate) { DigipostActionFactory = mockFactory.Object };
                var digipostApi = new DigipostApi(ClientConfig, Certificate, requestHelper);

                var result = digipostApi.Search(searchString);
                Assert.NotNull(result);
            }

        }
    }
}