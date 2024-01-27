using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Entrypoint;
using Digipost.Api.Client.Common.Relations;
using Digipost.Api.Client.Common.Share;
using Digipost.Api.Client.Common.Utilities;
using Digipost.Api.Client.Send;
using V8;

namespace Digipost.Api.Client.Api;

public interface IDocumentsApi
{
    /// <summary>
    /// Retrieves the status of a specific document asynchronously.
    /// </summary>
    /// <param name="guid">The unique identifier of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The status of the document.</returns>
    Task<DocumentStatus> GetDocumentStatusAsync(Guid guid, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the events of a specific document asynchronously from a specified time range.
    /// </summary>
    /// <param name="from">The starting date and time of the document events to retrieve.</param>
    /// <param name="to">The ending date and time of the document events to retrieve.</param>
    /// <param name="offset">The offset used for pagination of results.</param>
    /// <param name="maxResults">The maximum number of results to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of document events.</returns>
    Task<DocumentEvents> GetDocumentEventsAsync(DateTime from, DateTime to, int offset, int maxResults, CancellationToken cancellationToken);
}

public interface IShareDocumentsApi
{
    /// <summary>
    /// Gets the state of a share documents request asynchronously.
    /// </summary>
    /// <param name="guid">The unique identifier of the share documents request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// The task result is a <see cref="ShareDocumentsRequestState"/> object containing the state of the share documents request.
    /// </returns>
    Task<ShareDocumentsRequestState> GetShareDocumentsRequestStateAsync(Guid guid, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the content of a shared document asynchronously.
    /// </summary>
    /// <param name="uri">The <see cref="GetSharedDocumentContentUri"/> containing the URI of the shared document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="Task{SharedDocumentContent}"/> representing the asynchronous operation.
    /// The task result is a <see cref="SharedDocumentContent"/> object containing the content information of the shared document.
    /// </returns>
    Task<SharedDocumentContent> GetShareDocumentContentAsync(GetSharedDocumentContentUri uri, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the content of a shared document asynchronously.
    /// </summary>
    /// <param name="uri">The <see cref="GetSharedDocumentContentStreamUri"/> containing the URI of the shared document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="Task{Stream}"/> representing the asynchronous operation.
    /// The task result is a <see cref="Stream"/> object containing the content of the shared document.
    /// </returns>
    Task<Stream> FetchSharedDocumentAsync(GetSharedDocumentContentStreamUri uri, CancellationToken cancellationToken);
}

internal sealed class DocumentsApi(RequestHelper requestHelper, Root root, Sender sender)
    : IDocumentsApi, IShareDocumentsApi
{
    public async Task<DocumentStatus> GetDocumentStatusAsync(Guid guid, CancellationToken cancellationToken)
    {
        var documentStatusUri = root.GetDocumentStatusUri(guid);
        var result = await requestHelper.GetAsync<Document_Status>(documentStatusUri, cancellationToken);

        return result.FromDataTransferObject();
    }

    public async Task<DocumentEvents> GetDocumentEventsAsync(DateTime from, DateTime to, int offset, int maxResults, CancellationToken cancellationToken)
    {
        var documentEventsUri = root.GetDocumentEventsUri(sender, from, to, offset, maxResults);
        var result = await requestHelper.GetAsync<Document_Events>(documentEventsUri, cancellationToken);

        return result.FromDataTransferObject();
    }

    public async Task<ShareDocumentsRequestState> GetShareDocumentsRequestStateAsync(Guid guid, CancellationToken cancellationToken)
    {
        var shareDocumentsRequestStateUri = root.GetShareDocumentsRequestStateUri(guid);
        var result = await requestHelper.GetAsync<Share_Documents_Request_State>(shareDocumentsRequestStateUri, cancellationToken);

        return result.FromDataTransferObject();
    }
    
    public async Task<SharedDocumentContent> GetShareDocumentContentAsync(GetSharedDocumentContentUri uri, CancellationToken cancellationToken)
    {
        var result = await requestHelper.GetAsync<Shared_Document_Content>(uri, cancellationToken);

        return result.FromDataTransferObject();
    }

    public async Task<Stream> FetchSharedDocumentAsync(GetSharedDocumentContentStreamUri uri, CancellationToken cancellationToken)
    {
        return await requestHelper.GetStreamAsync(uri, cancellationToken);
    }
}