using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Fetches the archives.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous task that returns an IEnumerable of Archive objects.</returns>
    Task<List<Archive>> FetchArchivesAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Archives the given documents asynchronously.
    /// </summary>
    /// <param name="archiveWithDocuments">The Archive object containing the documents to be archived.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Task representing the asynchronous operation. The task result is the archived Archive object.</returns>
    Task<Archive> ArchiveDocumentsAsync(Archive archiveWithDocuments, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the archive documents based on the reference ID.
    /// </summary>
    /// <param name="referenceId">The reference ID to fetch the documents for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous task that returns an IEnumerable of Archive objects.</returns>
    Task<List<Archive>> FetchArchiveDocumentsByReferenceIdAsync(string referenceId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the archive document by UUID.
    /// </summary>
    /// <param name="getArchiveDocumentByUuidUri">The URI to fetch the archive document by UUID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The fetched archive document.</returns>
    Task<ArchiveDocument> FetchArchiveDocumentAsync(GetArchiveDocumentByUuidUri getArchiveDocumentByUuidUri, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches archive documents from the API.
    /// </summary>
    /// <param name="nextDocumentsUri">The URI representing the next page of archive documents to fetch.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result contains the fetched <see cref="Archive"/>.</returns>
    Task<Archive> FetchArchiveDocumentsAsync(ArchiveNextDocumentsUri nextDocumentsUri, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the given <paramref name="archiveDocument"/> with the provided <paramref name="updateUri"/>.
    /// </summary>
    /// <param name="archiveDocument">The document to be updated.</param>
    /// <param name="updateUri">The URI used to update the document.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the asynchronous operation.</param>
    /// <returns>Returns a Task representing the asynchronous operation.</returns>
    Task<ArchiveDocument> UpdateDocumentAsync(ArchiveDocument archiveDocument, ArchiveDocumentUpdateUri updateUri, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a document from the archive.
    /// </summary>
    /// <param name="deleteUri">The delete URI of the document to be deleted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteDocumentAsync(ArchiveDocumentDeleteUri deleteUri, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches an archive document from the Digipost archive based on the given UUID.
    /// </summary>
    /// <param name="getArchiveDocumentUri">The URI of the archive document to fetch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The fetched archive document.</returns>
    Task<Archive> GetArchiveDocumentAsync(GetArchiveDocumentByUuidUri getArchiveDocumentUri, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a document from the archive based on its external ID.
    /// </summary>
    /// <param name="externalId">The external ID of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous task that returns the fetched document.</returns>
    Task<Archive> FetchDocumentFromExternalIdAsync(string externalId, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the document from the external ID.
    /// </summary>
    /// <param name="externalId">The external ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous task that returns an Archive object.</returns>
    Task<Archive> FetchDocumentFromExternalIdAsync(Guid externalId, CancellationToken cancellationToken);

    /// <summary>
    /// Streams the document with the specified external ID.
    /// </summary>
    /// <param name="externalId">The external ID of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result will be the stream of the document.</returns>
    Task<Stream> StreamDocumentFromExternalIdAsync(string externalId, CancellationToken cancellationToken);

    /// <summary>
    /// Streams a document from the archive using the specified external ID.
    /// </summary>
    /// <param name="externalId">The external ID of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result will be a Stream object containing the document content.</returns>
    Task<Stream> StreamDocumentFromExternalIdAsync(Guid externalId, CancellationToken cancellationToken);

    /// <summary>
    /// Streams the document from the specified external ID.
    /// </summary>
    /// <param name="documentContentStreamUri">The external ID of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Task representing the asynchronous operation. The resulting Stream contains the document content.</returns>
    Task<Stream> StreamDocumentAsync(ArchiveDocumentContentStreamUri documentContentStreamUri, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the content of a document in the archive.
    /// </summary>
    /// <param name="archiveDocumentContentUri">The URI of the document content.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous task that returns an instance of ArchiveDocumentContent.</returns>
    Task<ArchiveDocumentContent> GetDocumentContentAsync(ArchiveDocumentContentUri archiveDocumentContentUri, CancellationToken cancellationToken);
}

internal sealed class ArchiveApi : IArchiveApi
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

    public async Task<List<Archive>> FetchArchivesAsync(CancellationToken cancellationToken)
    {
        var archivesUri = _root.GetGetArchivesUri();
        var archives = await _requestHelper.GetAsync<Archives>(archivesUri, cancellationToken);
        return archives?.Archive?.Select(x => x.FromDataTransferObject()).ToList() ?? [];
    }

    public async Task<List<Archive>> FetchArchiveDocumentsByReferenceIdAsync(string referenceId, CancellationToken cancellationToken)
    {
        var archives = await _requestHelper.GetAsync<Archives>(_root.GetGetArchiveDocumentsReferenceIdUri(referenceId), cancellationToken);
        return archives?.Archive?.Select(x => x.FromDataTransferObject()).ToList() ?? [];
    }

    public async Task<ArchiveDocument> FetchArchiveDocumentAsync(GetArchiveDocumentByUuidUri nextDocumentsUri, CancellationToken cancellationToken)
    {
        var archive = await GetArchiveDocumentAsync(nextDocumentsUri, cancellationToken);
        return archive.One();
    }

    public async Task<Archive> FetchArchiveDocumentsAsync(ArchiveNextDocumentsUri nextDocumentsUri, CancellationToken cancellationToken)
    {
        var result = await _requestHelper.GetAsync<V8.Archive>(nextDocumentsUri, cancellationToken);

        return result.FromDataTransferObject();
    }

    public async Task<Archive> GetArchiveDocumentAsync(GetArchiveDocumentByUuidUri getArchiveDocumentUri, CancellationToken cancellationToken)
    {
        var result = await _requestHelper.GetAsync<V8.Archive>(getArchiveDocumentUri, cancellationToken);
        return result.FromDataTransferObject();
    }

    public async Task<ArchiveDocument> UpdateDocumentAsync(ArchiveDocument archiveDocument, ArchiveDocumentUpdateUri updateUri, CancellationToken cancellationToken)
    {
        var messageAction = new ArchiveDocumentAction(archiveDocument);
        var httpContent = messageAction.Content(archiveDocument);

        var updatedArchiveDocument = await _requestHelper.PutAsync<Archive_Document>(httpContent, messageAction.RequestContent, updateUri, cancellationToken: cancellationToken);
        
        return updatedArchiveDocument.FromDataTransferObject();
    }

    public Task DeleteDocumentAsync(ArchiveDocumentDeleteUri deleteUri, CancellationToken cancellationToken) => _requestHelper.DeleteAsync(deleteUri, cancellationToken);

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

    public async Task<Archive> FetchDocumentFromExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        var result = await _requestHelper.GetAsync<V8.Archive>(_root.GetGetArchiveDocumentsByUuidUri(externalId), cancellationToken);
        return result.FromDataTransferObject();
    }

    public async Task<Archive> FetchDocumentFromExternalIdAsync(Guid externalId, CancellationToken cancellationToken)
    {
        var result = await _requestHelper.GetAsync<V8.Archive>(_root.GetGetArchiveDocumentsByUuidUri(externalId), cancellationToken);
        return result.FromDataTransferObject();
    }

    public async Task<Stream> StreamDocumentFromExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        var archive = await GetArchiveDocumentAsync(_root.GetGetArchiveDocumentsByUuidUri(externalId), cancellationToken);
        var documentContentStreamUri = archive.One().GetDocumentContentStreamUri();

        return await StreamDocumentAsync(documentContentStreamUri, cancellationToken);
    }

    public async Task<Stream> StreamDocumentFromExternalIdAsync(Guid externalId, CancellationToken cancellationToken)
    {
        var archive = await GetArchiveDocumentAsync(_root.GetGetArchiveDocumentsByUuidUri(externalId), cancellationToken);
        var documentContentStreamUri = archive.One().GetDocumentContentStreamUri();

        return await StreamDocumentAsync(documentContentStreamUri, cancellationToken);
    }

    public async Task<Stream> StreamDocumentAsync(ArchiveDocumentContentStreamUri documentContentStreamUri, CancellationToken cancellationToken)
    {
        return await _requestHelper.GetStreamAsync(documentContentStreamUri, cancellationToken);
    }

    public async Task<ArchiveDocumentContent> GetDocumentContentAsync(ArchiveDocumentContentUri archiveDocumentContentUri, CancellationToken cancellationToken)
    {
        var result = await _requestHelper.GetAsync<Archive_Document_Content>(archiveDocumentContentUri, cancellationToken);

        return result.FromDataTransferObject();
    }
}