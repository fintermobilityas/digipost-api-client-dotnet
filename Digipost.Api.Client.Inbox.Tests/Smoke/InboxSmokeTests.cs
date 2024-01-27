using System.Threading.Tasks;
using Digipost.Api.Client.Tests.Utilities;
using Xunit;
using Xunit.Sdk;

namespace Digipost.Api.Client.Inbox.Tests.Smoke
{
    public class InboxSmokeTests
    {
        public InboxSmokeTests()
        {
            _t = new InboxSmokeTestsHelper(SenderUtility.GetSender(TestEnvironment.Qa));
        }

        private readonly InboxSmokeTestsHelper _t;

        // To test this, log on to the account you are using and upload a document to the inbox.
        [Fact(Skip = "Skipping due to missing inbox for test users")]
        public async Task Get_inbox_and_read_document()
        {
            var inbox = await _t.Get_inbox();

            var fetchDocumentData = await inbox
                .Expect_inbox_to_have_documents()
                .Fetch_document_data();
            
            await fetchDocumentData
                .Delete_document();
        }
    }
}
