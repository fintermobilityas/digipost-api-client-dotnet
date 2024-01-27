using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Digipost.Api.Client.Common.Entrypoint;
using Digipost.Api.Client.Common.Relations;
using Digipost.Api.Client.Common.Utilities;

namespace Digipost.Api.Client.Inbox;

internal interface IInbox
{
    Task<IEnumerable<InboxDocument>> FetchAsync(int offset = 0, int limit = 100, CancellationToken cancellationToken = default);

    Task<Stream> FetchDocumentAsync(GetInboxDocumentContentUri document, CancellationToken cancellationToken);

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

    public async Task<IEnumerable<InboxDocument>> FetchAsync(int offset = 0, int limit = 100, CancellationToken cancellationToken = default)
    {
        var inboxPath = _inboxRoot.GetGetInboxUri(offset, limit);
        var result = await _requestHelper.GetAsync<V8.Inbox>(inboxPath, cancellationToken);
        return result.FromDataTransferObject();
    }

    public Task<Stream> FetchDocumentAsync(GetInboxDocumentContentUri getInboxDocumentContentUri, CancellationToken cancellationToken) => 
        _requestHelper.GetStreamAsync(getInboxDocumentContentUri, cancellationToken);

    public Task DeleteDocumentAsync(InboxDocumentDeleteUri deleteUri, CancellationToken cancellationToken) => _requestHelper.DeleteAsync(deleteUri, cancellationToken);
}