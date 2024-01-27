using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Digipost.Api.Client.Api;
using Digipost.Api.Client.Archive;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Entrypoint;
using Digipost.Api.Client.Common.Identify;
using Digipost.Api.Client.Common.Relations;
using Digipost.Api.Client.Common.Search;
using Digipost.Api.Client.Common.SenderInfo;
using Digipost.Api.Client.Common.Utilities;
using Digipost.Api.Client.Internal;
using Digipost.Api.Client.Send;
using Digipost.Api.Client.Shared.Certificate;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Digipost.Api.Client;

/// <summary>
/// Interface for interacting with the Digipost API.
/// </summary>
public interface IDigipostClient
{
    /// <summary>
    /// Retrieves the Root entrypoint, which is the starting point of the REST API of Digipost.
    /// </summary>
    /// <param name="apiRootUri">The API root URI.</param>
    /// <returns>The Root entrypoint.</returns>
    Task<Root> GetRootAsync(ApiRootUri apiRootUri);

    /// <summary>
    /// Fetches the sender information.
    /// </summary>
    /// <param name="sender">The sender is optional. If not specified, the broker will be used.</param>
    /// <returns>The sender information.</returns>
    Task<SenderInformation> GetSenderInformationAsync(Sender sender = null);

    /// <summary>
    /// Retrieves the sender information for the specified sender organisation.
    /// </summary>
    /// <param name="senderOrganisation">The sender organisation.</param>
    /// <returns>The sender information.</returns>
    Task<SenderInformation> GetSenderInformationAsync(SenderOrganisation senderOrganisation);

    /// <summary>
    /// Retrieves the inbox for the specified sender.
    /// </summary>
    /// <param name="senderId">The sender ID.</param>
    /// <returns>The inbox for the specified sender.</returns>
    Task<Inbox.Inbox> GetInboxAsync(Sender senderId);

    /// <summary>
    /// Get the archive API for a specific sender or all senders.
    /// </summary>
    /// <param name="senderId">The specific sender ID. Default is null to get archive API for all senders.</param>
    /// <returns>The archive API for the specific sender or all senders.</returns>
    Task<IArchiveApi> GetArchiveAsync(Sender senderId = null);

    /// <summary>
    /// Get access to the document api.
    /// </summary>
    /// <param name="sender">Optional parameter for sender if you are a broker. If you don't specify a sender, your broker ident will be used</param>
    /// <returns></returns>
    Task<IDocumentsApi> DocumentsApiAsync(Sender sender = null);

    /// <summary>
    /// Retrieves the API for managing documents in Digipost.
    /// </summary>
    /// <param name="sender">The sender of the message. If not specified, the default sender will be used.</param>
    /// <returns>The Documents API.</returns>
    Task<IDocumentsApi> GetDocumentApiAsync(Sender sender = null);

    /// <summary>
    /// Retrieves the instance of <see cref="IShareDocumentsApi"/> for sharing documents.
    /// </summary>
    /// <param name="sender">The sender of the message. Default is null.</param>
    /// <returns>The instance of <see cref="IShareDocumentsApi"/>.</returns>
    Task<IShareDocumentsApi> GetDocumentSharingAsync(Sender sender = null);

    /// <summary>
    /// Identifies the specified recipient.
    /// </summary>
    /// <param name="identification">The identification information of the recipient.</param>
    /// <returns>A task representing the asynchronous operation and containing the identification result.</returns>
    Task<IIdentificationResult> IdentifyAsyncAsync(IIdentification identification);

    /// <summary>
    /// Sends a message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>A task representing the asynchronous operation. The result of the task will be an instance of <see cref="IMessageDeliveryResult"/>.</returns>
    Task<IMessageDeliveryResult> SendMessageAsyncAsync(IMessage message);

    /// <summary>
    /// Adds additional data to a Digipost message.
    /// </summary>
    /// <param name="additionalData">The additional data to add to the message.</param>
    /// <param name="uri">The URI of the message to add the additional data to.</param>
    Task AddAdditionalDataAsync(AdditionalData additionalData, AddAdditionalDataUri uri);

    /// <summary>
    /// Searches for details using the given query.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>The search details result.</returns>
    Task<ISearchDetailsResult> SearchAsync(string query);
}

public sealed class DigipostClient : IDigipostClient
{
    readonly ClientConfig _clientConfig;
    readonly RequestHelper _requestHelper;
    readonly IMemoryCache _entrypointCache;

    readonly ILoggerFactory _loggerFactory;

    public DigipostClient(ClientConfig clientConfig, string thumbprint)
        : this(clientConfig, CertificateUtility.SenderCertificate(thumbprint), new NullLoggerFactory())
    {
    }

    public DigipostClient(ClientConfig clientConfig, X509Certificate2 enterpriseCertificate)
        : this(clientConfig, enterpriseCertificate, new NullLoggerFactory())
    {
    }

    public DigipostClient(ClientConfig clientConfig, X509Certificate2 enterpriseCertificate, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _entrypointCache = new MemoryCache(new MemoryCacheOptions());

        _clientConfig = clientConfig;
        var httpClient = GetHttpClient(enterpriseCertificate, clientConfig.WebProxy, clientConfig.Credential);
        _requestHelper = new RequestHelper(httpClient, _loggerFactory);
    }

    async Task<SendMessageApi> _sendMessageApi()
    {
        return new SendMessageApi(new SendRequestHelper(_requestHelper), _loggerFactory, await GetRootAsync(new ApiRootUri()));
    }

    HttpClient GetHttpClient(X509Certificate2 enterpriseCertificate, WebProxy proxy = null, NetworkCredential credential = null)
    {
        var allDelegationHandlers = new List<DelegatingHandler> { new AuthenticationHandler(_clientConfig, enterpriseCertificate, _loggerFactory) };

        var httpMessageHandler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        if (proxy != null)
        {
            proxy.Credentials = credential;
            httpMessageHandler.Proxy = proxy;
            httpMessageHandler.UseProxy = true;
            httpMessageHandler.UseDefaultCredentials = false;
        }
        var httpClient = HttpClientFactory.Create(
            httpMessageHandler,
            allDelegationHandlers.ToArray()
        );

        httpClient.Timeout = TimeSpan.FromMilliseconds(_clientConfig.TimeoutMilliseconds);
        httpClient.BaseAddress = new Uri(_clientConfig.Environment.Url.AbsoluteUri);

        return httpClient;
    }

    public async Task<Root> GetRootAsync(ApiRootUri apiRootUri)
    {
        var cacheKey = "root" + apiRootUri;

        if (_entrypointCache.TryGetValue(cacheKey, out Root root))
        {
            return root;
        }

        var entrypoint = await _requestHelper.GetAsync<V8.Entrypoint>(apiRootUri).ConfigureAwait(false);

        root = entrypoint.FromDataTransferObject();

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            // Keep in cache for 5 minutes when in use, but max 1 hour.
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        return _entrypointCache.Set(cacheKey, root, cacheEntryOptions);
    }

    /// <summary>
    /// Fetch Sender Information
    /// </summary>
    /// <param name="sender">The sender is optional. If not specified, the broker will be used.</param>
    /// <returns></returns>
    public async Task<SenderInformation> GetSenderInformationAsync(Sender sender = null)
    {
        var senderToUse = sender ?? new Sender(_clientConfig.Broker.Id);
        var root = await GetRootAsync(new ApiRootUri());
        var senderInformationUri = root.GetSenderInformationUri(senderToUse);

        return await new SenderInformationApi(_entrypointCache, _requestHelper).GetSenderInformation(senderInformationUri);
    }

    public async Task<SenderInformation> GetSenderInformationAsync(SenderOrganisation senderOrganisation)
    {
        var root = await GetRootAsync(new ApiRootUri());
        var senderInformationUri = root.GetSenderInformationUri(senderOrganisation.OrganisationNumber, senderOrganisation.PartId);

        return await new SenderInformationApi(_entrypointCache, _requestHelper).GetSenderInformation(senderInformationUri);
    }

    public async Task<Inbox.Inbox> GetInboxAsync(Sender senderId)
    {
        var root = await GetRootAsync(new ApiRootUri(senderId));
        return new Inbox.Inbox(_requestHelper, root);
    }

    public async Task<IArchiveApi> GetArchiveAsync(Sender senderId = null)
    {
        var root = await GetRootAsync(new ApiRootUri(senderId));
        return new Archive.ArchiveApi(_requestHelper, _loggerFactory, root);
    }

    /// <summary>
    /// Get access to the document api.
    /// </summary>
    /// <param name="sender">Optional parameter for sender if you are a broker. If you don't specify a sender, your broker ident will be used</param>
    /// <returns></returns>
    public async Task<IDocumentsApi> DocumentsApiAsync(Sender sender = null)
    {
        var senderToUse = sender ?? new Sender(_clientConfig.Broker.Id);
        var root = await GetRootAsync(new ApiRootUri(sender));
        return new DocumentsApi(_requestHelper, _loggerFactory, root, senderToUse);
    }

    public async Task<IDocumentsApi> GetDocumentApiAsync(Sender sender = null)
    {
        var root = await GetRootAsync(new ApiRootUri(sender));
        return new DocumentsApi(_requestHelper, _loggerFactory, root, sender);
    }

    public async Task<IShareDocumentsApi> GetDocumentSharingAsync(Sender sender = null)
    {
        var root = await GetRootAsync(new ApiRootUri(sender));
        return new DocumentsApi(_requestHelper, _loggerFactory, root, sender);
    }

    public async Task<IIdentificationResult> IdentifyAsyncAsync(IIdentification identification)
    {
        var sendMessageApi = await _sendMessageApi();
        return await sendMessageApi.IdentifyAsync(identification);
    }

    public async Task<IMessageDeliveryResult> SendMessageAsyncAsync(IMessage message)
    {
        var sendMessageApi = await _sendMessageApi();
        return await sendMessageApi.SendMessageAsync(message, _clientConfig.SkipMetaDataValidation);
    }

    public async Task AddAdditionalDataAsync(AdditionalData additionalData, AddAdditionalDataUri uri)
    {
        var sendMessageApi = await _sendMessageApi();
        await sendMessageApi.SendAdditionalDataAsync(additionalData, uri);
    }

    public async Task<ISearchDetailsResult> SearchAsync(string query)
    {
        var sendMessageApi = await _sendMessageApi();
        return await sendMessageApi.SearchAsync(query);
    }
}