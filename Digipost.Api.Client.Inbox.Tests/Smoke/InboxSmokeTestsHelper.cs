﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public InboxSmokeTestsHelper Get_inbox()
        {
            _inbox = _client.GetInbox(new Sender(_testSender.Id));
            _inboxDocuments = _inbox.Fetch().Result;

            return this;
        }

        public InboxSmokeTestsHelper Expect_inbox_to_have_documents()
        {
            Assert_state(_inboxDocuments);

            Assert.True(_inboxDocuments.Any());

            _inboxDocument = _inboxDocuments.First();

            return this;
        }

        public InboxSmokeTestsHelper Fetch_document_data()
        {
            Assert_state(_inboxDocument);

            var documentStream = _inbox.FetchDocument(_inboxDocument.GetGetDocumentContentUri()).Result;

            Assert.True(documentStream.CanRead);
            Assert.True(documentStream.Length > 500);

            return this;
        }

        public InboxSmokeTestsHelper Delete_document()
        {
            Assert_state(_inboxDocument);

            _inbox.DeleteDocument(_inboxDocument.GetDeleteUri()).Wait();

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
