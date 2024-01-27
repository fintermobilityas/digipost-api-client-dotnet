using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Digipost.Api.Client.Archive;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Relations;
using Xunit;

#pragma warning disable 0169
#pragma warning disable 0649

namespace Digipost.Api.Client.Docs;

public class ArchiveExamples
{
    static readonly DigipostClient Client;
    static readonly Sender Sender;

    async Task ArchiveADocument()
    {
        var archive = new Archive.Archive(new Sender(1234), new List<ArchiveDocument>
        {
            new ArchiveDocument(Guid.NewGuid(), "invoice_123123.pdf", "pdf", "application/psd", readFileFromDisk("invoice_123123.pdf")),
            new ArchiveDocument(Guid.NewGuid(), "attachment_123123.pdf", "pdf", "application/psd", readFileFromDisk("attachment_123123.pdf"))
        });

        var archiveApi = await Client.GetArchiveAsync();
        var savedArchive = archiveApi.ArchiveDocuments(archive);
    }

    async Task ArchiveADocumentWithAutoDelete()
    {
        var archive = new Archive.Archive(Sender, new List<ArchiveDocument>
        {
            new ArchiveDocument(Guid.NewGuid(), "invoice_123123.pdf", "pdf", "application/psd", readFileFromDisk("invoice_123123.pdf"))
                .WithDeletionTime(DateTime.Today.AddYears(5))
        });

        var archiveApi = await Client.GetArchiveAsync();
        var savedArchive = await archiveApi.ArchiveDocuments(archive);
    }

    async Task ArchiveADocumentToANamedArchive()
    {
        var archive = new Archive.Archive(Sender, new List<ArchiveDocument>(), "MyArchiveName");

        var archiveApi = await Client.GetArchiveAsync();
        var savedArchive = await archiveApi.ArchiveDocuments(archive);
    }

    async Task<IEnumerable<Archive.Archive>> FetchArchives()
    {
        var archiveApi = await Client.GetArchiveAsync();

        IEnumerable<Archive.Archive> fetchedArchives = await archiveApi.FetchArchives();
        return fetchedArchives;
    }

    async Task FetchAllArchiveDocuments()
    {
        var archiveApi = await Client.GetArchiveAsync();
        var current = (await archiveApi.FetchArchives()).First();
        var documents = new List<ArchiveDocument>();

        while (current.HasMoreDocuments())
        {
            var fetchArchiveDocuments = await archiveApi.FetchArchiveDocuments(current.GetNextDocumentsUri());
            documents.AddRange(fetchArchiveDocuments.ArchiveDocuments);
            current = fetchArchiveDocuments;
        }

        // documents now have all ArchiveDocuments in the archive
    }

    async Task FetchArchiveDocumentsWithAttribute()
    {
        var archiveApi = await Client.GetArchiveAsync();
        var archive = (await archiveApi.FetchArchives()).First();
        var searchBy = new Dictionary<string, string>
        {
            ["key"] = "val"
        };

        var fetchArchiveDocuments = await archiveApi.FetchArchiveDocuments(archive.GetNextDocumentsUri(searchBy));
        var documents = fetchArchiveDocuments.ArchiveDocuments;

        // documents now have the first 100 ArchiveDocuments in the archive that have invoicenumber=123123
    }

    async Task FetchAllArchiveDocumentsWithAttribute()
    {
        var archiveApi = await Client.GetArchiveAsync();
        var current = (await archiveApi.FetchArchives()).First();
        var documents = new List<ArchiveDocument>();

        var searchBy = new Dictionary<string, string>
        {
            ["key"] = "val"
        };

        while (current.HasMoreDocuments())
        {
            var fetchArchiveDocuments = await archiveApi.FetchArchiveDocuments(current.GetNextDocumentsUri(searchBy));
            documents.AddRange(fetchArchiveDocuments.ArchiveDocuments);
            current = fetchArchiveDocuments;
        }

        // documents now have all ArchiveDocuments in the archive that have invoicenumber=123123
    }

    async Task FetchArchiveDocumentByGuid()
    {
        var archiveApi = await Client.GetArchiveAsync(Sender);
        var root = await Client.GetRootAsync(new ApiRootUri());
        ArchiveDocument archiveDocument = await archiveApi.FetchArchiveDocument(root.GetGetArchiveDocumentsByUuidUri(Guid.Parse("10ff4c99-8560-4741-83f0-1093dc4deb1c")));
            
        ArchiveDocumentContent archiveDocumentContent = await archiveApi.GetDocumentContent(archiveDocument.DocumentContentUri());
        Uri uri = archiveDocumentContent.Uri;

        Stream streamDocumentFromExternalId = await archiveApi.StreamDocumentFromExternalId("My unique reference");
    }

    void ArchiveDocumentAttributes()
    {
        var archiveDocument = new ArchiveDocument(Guid.NewGuid(), "invoice_123123.pdf", "pdf", "application/psd", readFileFromDisk("invoice_123123.pdf"))
        {
            Attributes = {["invoicenumber"] = "123123"}
        };
    }

    async Task FetchArchiveDocumentsByReferenceId()
    {
        var archiveApi = await Client.GetArchiveAsync();
        IEnumerable<Archive.Archive> fetchArchiveDocumentsByReferenceId = await archiveApi.FetchArchiveDocumentsByReferenceId("MyProcessId[No12341234]");
    }

    async Task ChangeAttributesReferenceIdOnArchiveDocument()
    {
        var archiveApi = await Client.GetArchiveAsync(Sender);
        ArchiveDocument archiveDocument = (await archiveApi.FetchDocumentFromExternalId(Guid.Parse("10ff4c99-8560-4741-83f0-1093dc4deb1c"))).One();
        archiveDocument.WithAttribute("newKey", "foobar")
            .WithReferenceId("MyProcessId[No12341234]Done");

        ArchiveDocument updatedArchiveDocument = await archiveApi.UpdateDocument(archiveDocument, archiveDocument.GetUpdateUri());
    }

    async Task<IEnumerable<Archive.Archive>> AsBroker()
    {
        var archive = await Client.GetArchiveAsync(new Sender(111111));
        return await archive.FetchArchives();
    }

    byte[] readFileFromDisk(string invoicePdf)
    {
        throw new NotImplementedException();
    }
}