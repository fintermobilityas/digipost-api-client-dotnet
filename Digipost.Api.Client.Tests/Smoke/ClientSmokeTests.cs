using System;
using System.Threading.Tasks;
using Digipost.Api.Client.Common.Enums;
using Digipost.Api.Client.DataTypes.Core;
using Digipost.Api.Client.Send;
using Digipost.Api.Client.Tests.Utilities;
using Xunit;

namespace Digipost.Api.Client.Tests.Smoke
{
    public class ClientSmokeTests
    {
        private static ClientSmokeTestHelper _client;
        private static ClientSmokeTestHelper _clientWithoutDataTypes;

        public ClientSmokeTests()
        {
            var sender = SenderUtility.GetSender(TestEnvironment.Qa);
            _client = new ClientSmokeTestHelper(sender);
            _clientWithoutDataTypes = new ClientSmokeTestHelper(sender, true);
        }

        [Fact(Skip = "SmokeTest")]
        public async Task Can_identify_user()
        {
            var sendIdentification = await _client
                .HasRoot()
                .Create_identification_request()
                .SendIdentification();
            
            sendIdentification
                .Expect_identification_to_have_status(IdentificationResultType.DigipostAddress);
        }

        [Fact(Skip = "SmokeTest")]
        public async Task Can_Search()
        {
            var searchRequest = await _client.Create_search_request();
            searchRequest
                .Expect_search_to_have_result();
        }

        [Fact(Skip = "SmokeTest")]
        public async Task Get_sender_information()
        {
            await _client.SetSenderInformation();

            _client.Expect_valid_sender_information();
        }


        [Fact(Skip = "SmokeTest")]
        public async Task Get_Document_events()
        {
            var documentEvents = await _client.GetDocumentEvents();
            documentEvents
                .Expect_document_events();
        }


        [Fact(Skip = "SmokeTest")]
        public async Task Can_send_document_digipost_user()
        {
            var sendMessage = await _client
                .Create_message_with_primary_document()
                .To_Digital_Recipient()
                .SendMessage();
            
            sendMessage
                .Expect_message_to_have_status(MessageStatus.Delivered);
        }

        [Fact(Skip = "SmokeTest")]
        public async Task Can_send_invoice_digipost_user()
        {
            var invoice = new Invoice(dueDate: DateTime.Today.AddDays(14), sum: new decimal(100.21), creditorAccount: "2593143xxxx")
            {
                Kid = "123123123"
            };

            var sendMessage = await _client
                .CreateMessageWithPrimaryDataTypeDocument(invoice)
                .To_Digital_Recipient()
                .SendMessage();
            
            sendMessage
                .Expect_message_to_have_status(MessageStatus.Delivered);
        }

        [Fact(Skip = "SmokeTest")]
        public async Task Can_send_direct_to_print()
        {
            var sendPrintMessage = await _client
                .Create_message_with_primary_document()
                .ToPrintDirectly()
                .SendPrintMessage();
            
            sendPrintMessage
                .Expect_message_to_have_status(MessageStatus.DeliveredToPrint);
        }

        [Fact(Skip = "SmokeTest")]
        public async Task Can_send_request_for_registration()
        {
            var sendRequestForRegistration = await _client
                .Create_message_with_primary_document()
                .RequestForRegistration("+4711223344")
                // Find a user with Tenor testdata to test registration. This is TYKKHUDET EKTEMANN
                .SendRequestForRegistration("05926398190");
            
            sendRequestForRegistration
                .Expect_message_to_have_method(DeliveryMethod.PENDING);

            var documentEvents = await _client.GetDocumentEvents();
            
            documentEvents
                .Expect_document_events();
        }

        [Fact(Skip = "SmokeTest")]
        public async Task Can_send_document_with_object_datatype_to_digipost_user()
        {
            var externalLink = new ExternalLink(new Uri("https://www.test.no", UriKind.Absolute))
            {
                Description = "This is a link"
            };
            var linkXml = externalLink;

            var sendMessage = await _client
                .CreateMessageWithPrimaryDataTypeDocument(linkXml)
                .To_Digital_Recipient()
                .SendMessage();
            
            sendMessage
                .Expect_message_to_have_status(MessageStatus.Delivered);
        }

        [Fact(Skip = "SmokeTest")]
        public async Task Can_send_document_and_check_document_status()
        {
            var sendMessage = await _client
                .Create_message_with_primary_document()
                .To_Digital_Recipient()
                .SendMessage();
            
            sendMessage
                .Expect_message_to_have_status(MessageStatus.Delivered);

            var fetchDocumentStatus = await _client.FetchDocumentStatus();
            
            fetchDocumentStatus
                .Expect_document_status_to_be(
                    DocumentStatus.DocumentDeliveryStatus.DELIVERED,
                    DeliveryMethod.Digipost
                );
        }

        [Fact(Skip = "SmokeTest")]
        public async Task Check_document_status()
        {
            await _client.FetchDocumentStatus(Guid.Parse("92c95fa4-dc74-4196-95e9-4dc580017588")); //Use a guid that you know of. This is just a random one.
            
            _client.Expect_document_status_to_be(
                    DocumentStatus.DocumentDeliveryStatus.NOT_DELIVERED,
                    DeliveryMethod.PENDING
                );
        }

        [Fact(Skip = "SmokeTest")]
        public async Task Can_send_document_share_request_to_user()
        {
            var shareDocReq = new ShareDocumentsRequest(maxShareDurationSeconds: 60 * 60 * 24 * 2, purpose: "The purpose for my use of the document"); // Two days

            var sendMessage = await _client
                .CreateMessageWithPrimaryDataTypeDocument(shareDocReq)
                .To_Digital_Recipient()
                .SendMessage();
            
            sendMessage
                .Expect_message_to_have_status(MessageStatus.Delivered);

            var fetchDocumentStatus = await _client.FetchDocumentStatus();
            
            fetchDocumentStatus
                .Expect_document_status_to_be(
                    DocumentStatus.DocumentDeliveryStatus.DELIVERED,
                    DeliveryMethod.Digipost
                );

            //Set breakpoint her og del et dokument i qa.digipost.no!
            var fetchShareDocumentRequestState = await _client.FetchShareDocumentRequestState();
            
            await fetchShareDocumentRequestState
                .ExpectShareDocumentsRequestState();

            await _client.PerformStopSharing();
        }
    }
}
