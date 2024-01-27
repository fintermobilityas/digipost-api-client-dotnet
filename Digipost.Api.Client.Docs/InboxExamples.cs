﻿using System.Linq;
using System.Threading.Tasks;
using Digipost.Api.Client.Common;
#pragma warning disable 0649

namespace Digipost.Api.Client.Docs;

public class InboxExamples
{
    static readonly DigipostClient client;
    static readonly Sender sender;

    public void ClientConfiguration()
    {
        // The actual sender of the message. The broker is the owner of the organization certificate
        // used in the library. The broker id can be retrieved from your Digipost organization account.
        var broker = new Broker(12345);

        // The sender is what the receiver of the message sees as the sender of the message.
        // Sender and broker id will both be your organization's id if you are sending on behalf of yourself.
        var sender = new Sender(67890);

        var clientConfig = new ClientConfig(broker, Environment.Production);

        var client = new DigipostClient(clientConfig, CertificateReader.ReadCertificate());
    }

    async Task Hent_dokumenter()
    {
        var inbox = await client.GetInboxAsync(sender);

        var first100 = inbox.Fetch(); //Default offset is 0 and default limit is 100

        var next200 = inbox.Fetch(100, 100);
    }

    async Task Hent_dokumentinnhold()
    {
        var inbox = await client.GetInboxAsync(sender);

        var documentMetadata = (await inbox.Fetch()).First();

        var documentStream = await inbox.FetchDocument(documentMetadata.GetGetDocumentContentUri());
    }

    async Task Slett_dokument()
    {
        var inbox = await client.GetInboxAsync(sender);

        var documentMetadata = (await inbox.Fetch()).First();

        await inbox.DeleteDocument(documentMetadata.GetDeleteUri());
    }
}