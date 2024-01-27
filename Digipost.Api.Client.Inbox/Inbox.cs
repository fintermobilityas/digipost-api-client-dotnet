using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Digipost.Api.Client.Common.Entrypoint;
using Digipost.Api.Client.Common.Relations;
using Digipost.Api.Client.Common.Utilities;

namespace Digipost.Api.Client.Inbox;

internal interface IInbox
{
    /// <summary>
    /// Fetches a collection of InboxDocuments asynchronously.
    /// </summary>
    /// <param name="offset">The offset of documents to fetch (optional, default is 0).</param>
    /// <param name="limit">The maximum number of documents to fetch (optional, default is 100).</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation (optional, default is CancellationToken.None).</param>
    /// <returns>Returns a task representing the asynchronous operation that returns a collection of InboxDocuments.</returns>
    Task<List<InboxDocument>> FetchAsync(int offset = 0, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches a document from the inbox asynchronously.
    /// </summary>
    /// <param name="document">The document to fetch.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation (optional, default is CancellationToken.None).</param>
    /// <returns>Returns a task representing the asynchronous operation that returns a stream containing the document content.</returns>
    Task<Stream> FetchDocumentAsync(GetInboxDocumentContentUri document, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a document from the inbox asynchronously.
    /// </summary>
    /// <param name="document">The document to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation (optional, default is CancellationToken.None).</param>
    /// <returns>Returns a task representing the asynchronous operation.</returns>
    Task DeleteDocumentAsync(InboxDocumentDeleteUri document, CancellationToken cancellationToken);
}

public sealed class Inbox : IInbox
{
    readonly Root _inboxRoot;
    readonly RequestHelper _requestHelper;

    internal Inbox(RequestHelper requestHelper, Root root)
    {
        _inboxRoot = root;
        _requestHelper = requestHelper;
    }

    public async Task<List<InboxDocument>> FetchAsync(int offset = 0, int limit = 100, CancellationToken cancellationToken = default)
    {
        var inboxPath = _inboxRoot.GetGetInboxUri(offset, limit);
        var result = await _requestHelper.GetAsync<V8.Inbox>(inboxPath, cancellationToken);
        return result.FromDataTransferObject().ToList();
    }

    public Task<Stream> FetchDocumentAsync(GetInboxDocumentContentUri getInboxDocumentContentUri, CancellationToken cancellationToken) => 
        _requestHelper.GetStreamAsync(getInboxDocumentContentUri, cancellationToken);

    public Task DeleteDocumentAsync(InboxDocumentDeleteUri deleteUri, CancellationToken cancellationToken) => _requestHelper.DeleteAsync(deleteUri, cancellationToken);
}