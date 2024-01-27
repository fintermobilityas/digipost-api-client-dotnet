using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Digipost.Api.Client.Archive.Actions;
using Digipost.Api.Client.Common.Entrypoint;
using Digipost.Api.Client.Common.Relations;
using Digipost.Api.Client.Common.Utilities;
using Microsoft.Extensions.Logging;
using V8;

namespace Digipost.Api.Client.Archive;

public interface IArchiveApi
{
    /// <summary>
    /// List all the archives available to the current sender
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<Archive>> FetchArchives(CancellationToken cancellationToken);

    Task<Archive> ArchiveDocuments(Archive archiveWithDocument, CancellationToken cancellationToken);

    Task<Archive> ArchiveDocumentsAsync(Archive archiveWithDocuments, CancellationToken cancellationToken);

    Task<IEnumerable<Archive>> FetchArchiveDocumentsByReferenceId(string referenceId, CancellationToken cancellationToken);

    Task<ArchiveDocument> FetchArchiveDocument(GetArchiveDocumentByUuidUri getArchiveDocumentByUuidUri, CancellationToken cancellationToken);

    Task<Archive> FetchArchiveDocuments(ArchiveNextDocumentsUri nextDocumentsUri, CancellationToken cancellationToken);

    Task<ArchiveDocument> UpdateDocument(ArchiveDocument archiveDocument, ArchiveDocumentUpdateUri updateUri, CancellationToken cancellationToken);

    Task DeleteDocument(ArchiveDocumentDeleteUri deleteUri, CancellationToken cancellationToken);

    /**
     * This will hash and create a Guid the same way as java UUID.nameUUIDFromBytes
     */
    Task<Archive> GetArchiveDocument(GetArchiveDocumentByUuidUri getArchiveDocumentUri, CancellationToken cancellationToken);

    Task<Archive> FetchDocumentFromExternalId(string externalId, CancellationToken cancellationToken);

    Task<Archive> FetchDocumentFromExternalId(Guid externalIdGuid, CancellationToken cancellationToken);

    Task<Stream> StreamDocumentFromExternalId(string externalId, CancellationToken cancellationToken);

    Task<Stream> StreamDocumentFromExternalId(Guid externalIdGuid, CancellationToken cancellationToken);

    Task<Stream> StreamDocument(ArchiveDocumentContentStreamUri documentContentStreamUri, CancellationToken cancellationToken);

    Task<ArchiveDocumentContent> GetDocumentContent(ArchiveDocumentContentUri archiveDocumentContentUri, CancellationToken cancellationToken);
}

internal class ArchiveApi : IArchiveApi
{
    readonly Root _root;
    readonly RequestHelper _requestHelper;
    readonly ILogger<ArchiveApi> _logger;

    internal ArchiveApi(RequestHelper requestHelper, ILoggerFactory loggerFactory, Root root)
    {
        _root = root;
        _logger = loggerFactory.CreateLogger<ArchiveApi>();
        _requestHelper = requestHelper;
    }

    public async Task<IEnumerable<Archive>> FetchArchives(CancellationToken cancellationToken)
    {
        var archivesUri = _root.GetGetArchivesUri();
        var archives = await _requestHelper.GetAsync<Archives>(archivesUri, cancellationToken);

        return archives.Archive.Select(ArchiveDataTransferObjectConverter.FromDataTransferObject);
    }

    public async Task<IEnumerable<Archive>> FetchArchiveDocumentsByReferenceId(string referenceId, CancellationToken cancellationToken)
    {
        var archives = await _requestHelper.GetAsync<Archives>(_root.GetGetArchiveDocumentsReferenceIdUri(referenceId), cancellationToken);

        return archives.Archive.Select(ArchiveDataTransferObjectConverter.FromDataTransferObject);
    }

    public async Task<ArchiveDocument> FetchArchiveDocument(GetArchiveDocumentByUuidUri nextDocumentsUri, CancellationToken cancellationToken)
    {
        var archive = await GetArchiveDocument(nextDocumentsUri, cancellationToken);
        return archive.One();
    }

    public async Task<Archive> FetchArchiveDocuments(ArchiveNextDocumentsUri nextDocumentsUri, CancellationToken cancellationToken)
    {
        var result = await _requestHelper.GetAsync<V8.Archive>(nextDocumentsUri, cancellationToken);

        return result.FromDataTransferObject();
    }

    public async Task<Archive> GetArchiveDocument(GetArchiveDocumentByUuidUri getArchiveDocumentUri, CancellationToken cancellationToken)
    {
        var result = await _requestHelper.GetAsync<V8.Archive>(getArchiveDocumentUri, cancellationToken);
        return result.FromDataTransferObject();
    }

    public async Task<ArchiveDocument> UpdateDocument(ArchiveDocument archiveDocument, ArchiveDocumentUpdateUri updateUri, CancellationToken cancellationToken)
    {
        var messageAction = new ArchiveDocumentAction(archiveDocument);
        var httpContent = messageAction.Content(archiveDocument);

        var updatedArchiveDocument = await _requestHelper.PutAsync<Archive_Document>(httpContent, messageAction.RequestContent, updateUri, cancellationToken: cancellationToken);
        
        return updatedArchiveDocument.FromDataTransferObject();
    }

    public async Task DeleteDocument(ArchiveDocumentDeleteUri deleteUri, CancellationToken cancellationToken)
    {
        await _requestHelper.DeleteAsync(deleteUri, cancellationToken);
    }

    public async Task<Archive> ArchiveDocuments(Archive archiveWithDocuments, CancellationToken cancellationToken)
    {
        var result = await ArchiveDocumentsAsync(archiveWithDocuments, cancellationToken);
        return result;
    }

    public async Task<Archive> ArchiveDocumentsAsync(Archive archiveWithDocuments, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Outgoing archive '{count}' documents to archive: {name}", archiveWithDocuments.ArchiveDocuments.Count, archiveWithDocuments.Name ?? "default");

        var archiveUri = _root.GetArchiveDocumentsUri();

        var archiveAction = new ArchiveAction(archiveWithDocuments);
        var httpContent = archiveAction.Content(archiveWithDocuments);

        var archiveDocument = await _requestHelper.PostAsync<V8.Archive>(httpContent, archiveAction.RequestContent, archiveUri, cancellationToken: cancellationToken);
        
        var result = archiveDocument.FromDataTransferObject();

        _logger.LogDebug("Response received for archiving to '{name}'", archiveWithDocuments.Name ?? "default");

        return result;
    }

    public async Task<Archive> FetchDocumentFromExternalId(string externalId, CancellationToken cancellationToken)
    {
        var result = await _requestHelper.GetAsync<V8.Archive>(_root.GetGetArchiveDocumentsByUuidUri(externalId), cancellationToken);
        return result.FromDataTransferObject();
    }

    public async Task<Archive> FetchDocumentFromExternalId(Guid externalIdGuid, CancellationToken cancellationToken)
    {
        var result = await _requestHelper.GetAsync<V8.Archive>(_root.GetGetArchiveDocumentsByUuidUri(externalIdGuid), cancellationToken);
        return result.FromDataTransferObject();
    }

    public async Task<Stream> StreamDocumentFromExternalId(string externalId, CancellationToken cancellationToken)
    {
        var archive = await GetArchiveDocument(_root.GetGetArchiveDocumentsByUuidUri(externalId), cancellationToken);
        var documentContentStreamUri = archive.One().GetDocumentContentStreamUri();

        return await StreamDocument(documentContentStreamUri, cancellationToken);
    }

    public async Task<Stream> StreamDocumentFromExternalId(Guid guid, CancellationToken cancellationToken)
    {
        var archive = await GetArchiveDocument(_root.GetGetArchiveDocumentsByUuidUri(guid), cancellationToken);
        var documentContentStreamUri = archive.One().GetDocumentContentStreamUri();

        return await StreamDocument(documentContentStreamUri, cancellationToken);
    }

    public async Task<Stream> StreamDocument(ArchiveDocumentContentStreamUri documentContentStreamUri, CancellationToken cancellationToken)
    {
        return await _requestHelper.GetStreamAsync(documentContentStreamUri, cancellationToken);
    }

    public async Task<ArchiveDocumentContent> GetDocumentContent(ArchiveDocumentContentUri archiveDocumentContentUri, CancellationToken cancellationToken)
    {
        var result = await _requestHelper.GetAsync<Archive_Document_Content>(archiveDocumentContentUri, cancellationToken);

        return result.FromDataTransferObject();
    }
}