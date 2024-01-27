using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Utilities;
using Digipost.Api.Client.Shared.Tests;
using Digipost.Api.Client.Tests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Digipost.Api.Client.Inbox.Tests.Smoke
{
    public class InboxSmokeTestsHelper
    {
        private readonly DigipostClient _client;
        private readonly TestSender _testSender;
        private Inbox _inbox;
        private InboxDocument _inboxDocument;
        private IEnumerable<InboxDocument> _inboxDocuments;

        internal InboxSmokeTestsHelper(TestSender testSender)
        {
            _testSender = testSender;
            var broker = new Broker(testSender.Id);

            var serviceProvider = LoggingUtility.CreateServiceProviderAndSetUpLogging();

            _client = new DigipostClient(
                new ClientConfig(broker, testSender.Environment),
                testSender.Certificate,
                serviceProvider.GetService<ILoggerFactory>()
            );
        }

        public async Task<InboxSmokeTestsHelper> Get_inbox()
        {
            _inbox = await _client.GetInbox(new Sender(_testSender.Id));
            _inboxDocuments = await _inbox.Fetch();

            return this;
        }

        public InboxSmokeTestsHelper Expect_inbox_to_have_documents()
        {
            Assert_state(_inboxDocuments);

            Assert.True(_inboxDocuments.Any());

            _inboxDocument = _inboxDocuments.First();

            return this;
        }

        public async Task<InboxSmokeTestsHelper> Fetch_document_data()
        {
            Assert_state(_inboxDocument);

            var documentStream = await _inbox.FetchDocument(_inboxDocument.GetGetDocumentContentUri());

            Assert.True(documentStream.CanRead);
            Assert.True(documentStream.Length > 500);

            return this;
        }

        public async Task<InboxSmokeTestsHelper> Delete_document()
        {
            Assert_state(_inboxDocument);

            await _inbox.DeleteDocument(_inboxDocument.GetDeleteUri());

            return this;
        }

        private static void Assert_state(object obj)
        {
            if (obj == null)
            {
                throw new InvalidOperationException("Requires gradually built state. Make sure you use functions in the correct order.");
            }
        }
    }
}
