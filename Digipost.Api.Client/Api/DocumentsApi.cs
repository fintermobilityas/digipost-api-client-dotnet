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
    Task<DocumentStatus> GetDocumentStatusAsync(Guid guid, CancellationToken cancellationToken);
    Task<DocumentEvents> GetDocumentEventsAsync(DateTime from, DateTime to, int offset, int maxResults, CancellationToken cancellationToken);
}
public interface IShareDocumentsApi
{
    Task<ShareDocumentsRequestState> GetShareDocumentsRequestStateAsync(Guid guid, CancellationToken cancellationToken);
    Task<SharedDocumentContent> GetShareDocumentContentAsync(GetSharedDocumentContentUri uri, CancellationToken cancellationToken);
    Task<Stream> FetchSharedDocumentAsync(GetSharedDocumentContentStreamUri uri, CancellationToken cancellationToken);
}

internal class DocumentsApi(RequestHelper requestHelper, Root root, Sender sender)
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