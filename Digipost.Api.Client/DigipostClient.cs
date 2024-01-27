using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Digipost.Api.Client;

/// <summary>
/// Interface for interacting with the Digipost API.
/// </summary>
public interface IDigipostClient
{
    /// <summary>
    /// The sender of the message, i.e. what the receiver of the message sees as the sender of the message.
    /// </summary>
    public Sender Sender { get; }

    /// <summary>
    /// Contains configuration for sending digital post.
    /// </summary>
    public ClientConfig ClientConfig { get; }
    
    /// <summary>
    /// Retrieves the Root entrypoint, which is the starting point of the REST API of Digipost.
    /// </summary>
    /// <param name="apiRootUri">The API root URI.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The Root entrypoint.</returns>
    Task<Root> GetRootAsync(ApiRootUri apiRootUri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches the sender information.
    /// </summary>
    /// <param name="sender">The sender is optional. If not specified, the broker will be used.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The sender information.</returns>
    Task<SenderInformation> GetSenderInformationAsync(Sender sender = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the sender information for the specified sender organisation.
    /// </summary>
    /// <param name="senderOrganisation">The sender organisation.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The sender information.</returns>
    Task<SenderInformation> GetSenderInformationAsync(SenderOrganisation senderOrganisation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the inbox for the specified sender.
    /// </summary>
    /// <param name="senderId">The sender ID.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The inbox for the specified sender.</returns>
    Task<Inbox.Inbox> GetInboxAsync(Sender senderId, CancellationToken cancellationToken);

    /// <summary>
    /// Get the archive API for a specific sender or all senders.
    /// </summary>
    /// <param name="senderId">The specific sender ID. Default is null to get archive API for all senders.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The archive API for the specific sender or all senders.</returns>
    Task<IArchiveApi> GetArchiveAsync(Sender senderId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get access to the document api.
    /// </summary>
    /// <param name="sender">Optional parameter for sender if you are a broker. If you don't specify a sender, your broker ident will be used</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IDocumentsApi> DocumentsApiAsync(Sender sender = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the API for managing documents in Digipost.
    /// </summary>
    /// <param name="sender">The sender of the message. If not specified, the default sender will be used.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The Documents API.</returns>
    Task<IDocumentsApi> GetDocumentApiAsync(Sender sender = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the instance of <see cref="IShareDocumentsApi"/> for sharing documents.
    /// </summary>
    /// <param name="sender">The sender of the message. Default is null.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The instance of <see cref="IShareDocumentsApi"/>.</returns>
    Task<IShareDocumentsApi> GetDocumentSharingAsync(Sender sender = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Identifies the specified recipient.
    /// </summary>
    /// <param name="identification">The identification information of the recipient.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation and containing the identification result.</returns>
    Task<IIdentificationResult> IdentifyAsync(IIdentification identification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation. The result of the task will be an instance of <see cref="IMessageDeliveryResult"/>.</returns>
    Task<IMessageDeliveryResult> SendMessageAsync(IMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds additional data to a Digipost message.
    /// </summary>
    /// <param name="additionalData">The additional data to add to the message.</param>
    /// <param name="uri">The URI of the message to add the additional data to.</param>
    /// <param name="cancellationToken"></param>
    Task AddAdditionalDataAsync(AdditionalData additionalData, AddAdditionalDataUri uri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for details using the given query.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The search details result.</returns>
    Task<ISearchDetailsResult> SearchAsync(string query, CancellationToken cancellationToken = default);
}

public sealed class DigipostClient : IDigipostClient
{
    readonly RequestHelper _requestHelper;
    readonly IMemoryCache _entrypointCache;
    readonly ILoggerFactory _loggerFactory;
    
    public Sender Sender => ClientConfig.Sender;
    public ClientConfig ClientConfig { get; }

    public DigipostClient(ClientConfig clientConfig, X509Certificate2 certificate, ILoggerFactory loggerFactory) : 
        this(clientConfig, BuildNonPooledHttpClient(clientConfig, certificate, loggerFactory), loggerFactory)
    {

    }
    
    internal DigipostClient(ClientConfig clientConfig, HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _entrypointCache = new MemoryCache(new MemoryCacheOptions());

        ClientConfig = clientConfig;
        _requestHelper = new RequestHelper(httpClient, _loggerFactory);
    }

    async Task<SendMessageApi> _sendMessageApi()
    {
        var root = await GetRootAsync(new ApiRootUri());
        return new SendMessageApi(new SendRequestHelper(_requestHelper), _loggerFactory, root);
    }

    static HttpClient BuildNonPooledHttpClient(ClientConfig clientConfig, X509Certificate2 certificate, ILoggerFactory loggerFactory)
    {
        if (!certificate.HasPrivateKey)
        {
            throw new InvalidOperationException("Certificate must contain a private key.");
        }
        
        var httpMessageHandler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        if (clientConfig.WebProxy != null)
        {
            httpMessageHandler.Proxy = clientConfig.WebProxy;
            httpMessageHandler.UseProxy = true;
            httpMessageHandler.UseDefaultCredentials = false;
        }

        var authenticationHandler = new AuthenticationHandler(clientConfig, certificate, loggerFactory);
        authenticationHandler.InnerHandler = httpMessageHandler;

        var httpClient = new HttpClient(authenticationHandler);

        httpClient.Timeout = TimeSpan.FromMilliseconds(clientConfig.TimeoutMilliseconds);
        httpClient.BaseAddress = new Uri(clientConfig.Environment.Url.AbsoluteUri);

        return httpClient;
    }

    public async Task<Root> GetRootAsync(ApiRootUri apiRootUri, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"root{apiRootUri}";

        if (_entrypointCache.TryGetValue(cacheKey, out Root root))
        {
            return root;
        }

        var entrypoint = await _requestHelper.GetAsync<V8.Entrypoint>(apiRootUri, cancellationToken);

        root = entrypoint.FromDataTransferObject();

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            // Keep in cache for 5 minutes when in use, but max 1 hour.
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        return _entrypointCache.Set(cacheKey, root, cacheEntryOptions);
    }
    
    public async Task<SenderInformation> GetSenderInformationAsync(Sender sender = null, CancellationToken cancellationToken = default)
    {
        var senderToUse = sender ?? new Sender(ClientConfig.Broker.Id);
        var root = await GetRootAsync(new ApiRootUri(), cancellationToken);
        var senderInformationUri = root.GetSenderInformationUri(senderToUse);

        return await new SenderInformationApi(_entrypointCache, _requestHelper).GetSenderInformationAsync(senderInformationUri, cancellationToken);
    }

    public async Task<SenderInformation> GetSenderInformationAsync(SenderOrganisation senderOrganisation, CancellationToken cancellationToken)
    {
        var root = await GetRootAsync(new ApiRootUri(), cancellationToken);
        var senderInformationUri = root.GetSenderInformationUri(senderOrganisation.OrganisationNumber, senderOrganisation.PartId);

        return await new SenderInformationApi(_entrypointCache, _requestHelper).GetSenderInformationAsync(senderInformationUri, cancellationToken);
    }

    public async Task<Inbox.Inbox> GetInboxAsync(Sender senderId, CancellationToken cancellationToken)
    {
        var root = await GetRootAsync(new ApiRootUri(senderId), cancellationToken);
        return new Inbox.Inbox(_requestHelper, root);
    }

    public async Task<IArchiveApi> GetArchiveAsync(Sender senderId = null, CancellationToken cancellationToken = default)
    {
        var root = await GetRootAsync(new ApiRootUri(senderId), cancellationToken);
        return new ArchiveApi(_requestHelper, _loggerFactory, root);
    }
    
    public async Task<IDocumentsApi> DocumentsApiAsync(Sender sender = null, CancellationToken cancellationToken = default)
    {
        var senderToUse = sender ?? new Sender(ClientConfig.Broker.Id);
        var root = await GetRootAsync(new ApiRootUri(sender), cancellationToken);
        return new DocumentsApi(_requestHelper, root, senderToUse);
    }

    public async Task<IDocumentsApi> GetDocumentApiAsync(Sender sender = null, CancellationToken cancellationToken = default)
    {
        var root = await GetRootAsync(new ApiRootUri(sender), cancellationToken);
        return new DocumentsApi(_requestHelper, root, sender);
    }

    public async Task<IShareDocumentsApi> GetDocumentSharingAsync(Sender sender = null, CancellationToken cancellationToken = default)
    {
        var root = await GetRootAsync(new ApiRootUri(sender), cancellationToken);
        return new DocumentsApi(_requestHelper, root, sender);
    }

    public async Task<IIdentificationResult> IdentifyAsync(IIdentification identification, CancellationToken cancellationToken)
    {
        var sendMessageApi = await _sendMessageApi();
        return await sendMessageApi.IdentifyAsync(identification, cancellationToken);
    }

    public async Task<IMessageDeliveryResult> SendMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        var sendMessageApi = await _sendMessageApi();
        return await sendMessageApi.SendMessageAsync(message, ClientConfig.SkipMetaDataValidation, cancellationToken);
    }

    public async Task AddAdditionalDataAsync(AdditionalData additionalData, AddAdditionalDataUri uri, CancellationToken cancellationToken)
    {
        var sendMessageApi = await _sendMessageApi();
        await sendMessageApi.SendAdditionalDataAsync(additionalData, uri, cancellationToken);
    }

    public async Task<ISearchDetailsResult> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var sendMessageApi = await _sendMessageApi();
        return await sendMessageApi.SearchAsync(query, cancellationToken);
    }
}