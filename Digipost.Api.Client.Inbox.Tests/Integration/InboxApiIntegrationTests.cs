using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Digipost.Api.Client.Common.Entrypoint;
using Digipost.Api.Client.Common.Exceptions;
using Digipost.Api.Client.Common.Utilities;
using Digipost.Api.Client.Resources.Xml;
using Digipost.Api.Client.Tests;
using Digipost.Api.Client.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Digipost.Api.Client.Inbox.Tests.Integration;

public class InboxApiIntegrationTests
{
    readonly Inbox _inbox = GetInbox();

    static Inbox GetInbox()
    {
        var httpClient = new HttpClient(
            new FakeResponseHandler { ResultCode = HttpStatusCode.NotFound, HttpContent = XmlResource.Inbox.GetError() }
        )
        {
            BaseAddress = new Uri("http://www.fakeBaseAddress.no")
        };
        var requestHelper = new RequestHelper(httpClient, new NullLoggerFactory());

        var links = new Dictionary<string, Link>
        {
            ["GET_INBOX"] = new(httpClient.BaseAddress + $"{DomainUtility.GetSender().Id}/inbox") { Rel = httpClient.BaseAddress + "relations/get_inbox" }
        };
        var root = new Root("")
        {
            Links = links
        };

        var inbox = new Inbox(requestHelper, root);
        return inbox;
    }

    public class FetchMethod : InboxApiIntegrationTests
    {
        [Fact]
        public Task ErrorShouldCauseDigipostResponseException() =>
            Assert.ThrowsAsync<ClientResponseException>(() => _inbox.FetchAsync());
    }
}