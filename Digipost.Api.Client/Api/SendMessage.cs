using System.Collections.Generic;
using System.Threading.Tasks;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Entrypoint;
using Digipost.Api.Client.Common.Identify;
using Digipost.Api.Client.Common.Relations;
using Digipost.Api.Client.Common.Search;
using Digipost.Api.Client.Extensions;
using Digipost.Api.Client.Send;
using Microsoft.Extensions.Logging;
using V8;

namespace Digipost.Api.Client.Api;

internal class SendMessageApi
{
    readonly Root _root;
    const int MinimumSearchLength = 3;
    readonly ILogger<SendMessageApi> _logger;

    public SendMessageApi(SendRequestHelper requestHelper, ILoggerFactory loggerFactory, Root root)
    {
        _root = root;
        _logger = loggerFactory.CreateLogger<SendMessageApi>();
        RequestHelper = requestHelper;
    }

    public SendRequestHelper RequestHelper { get; }

    public async Task<IMessageDeliveryResult> SendMessageAsync(IMessage message, bool skipMetaDataValidation = false)
    {
        _logger.LogDebug("Outgoing Digipost message to Recipient: {message}", message);

        var messageDelivery = await RequestHelper.PostMessage<Message_Delivery>(message, _root.GetSendMessageUri(), skipMetaDataValidation);

        var messageDeliveryResult = messageDelivery.FromDataTransferObject();

        _logger.LogDebug("Response received for message to recipient, {message}: '{status}'. Will be available to Recipient at {deliverytime}.", message, messageDeliveryResult.Status, messageDeliveryResult.DeliveryTime);

        return messageDeliveryResult;
    }

    public async Task<IIdentificationResult> IdentifyAsync(IIdentification identification)
    {
        _logger.LogDebug("Outgoing identification request: {identification}", identification);

        var identificationResultDataTransferObject = await RequestHelper.PostIdentification<Identification_Result>(identification, _root.GetIdentifyRecipientUri());
        var identificationResult = identificationResultDataTransferObject.FromDataTransferObject();

        _logger.LogDebug("Response received for identification to recipient, ResultType '{resultType}', Data '{data}'.", identificationResult.ResultType, identificationResult.Data);

        return identificationResult;
    }

    public async Task<ISearchDetailsResult> SearchAsync(string search)
    {
        _logger.LogDebug("Outgoing search request, term: '{search}'.", search);

        search = search.RemoveReservedUriCharacters();
        var uri = _root.GetRecipientSearchUri(search);

        if (search.Length < MinimumSearchLength)
        {
            var emptyResult = new SearchDetailsResult { PersonDetails = new List<SearchDetails>() };

            var taskSource = new TaskCompletionSource<ISearchDetailsResult>();
            taskSource.SetResult(emptyResult);
            return await taskSource.Task.ConfigureAwait(false);
        }

        var searchDetailsResultDataTransferObject = await RequestHelper.Get<Recipients>(uri).ConfigureAwait(false);

        var searchDetailsResult = searchDetailsResultDataTransferObject.FromDataTransferObject();

        _logger.LogDebug("Response received for search with term '{search}' retrieved.", search);

        return searchDetailsResult;
    }

    public async Task SendAdditionalDataAsync(IAdditionalData additionalData, AddAdditionalDataUri uri)
    {
        _logger.LogDebug("Sending additional data '{uri}'", uri);

        await RequestHelper.PostAdditionalData<string>(additionalData, uri).ConfigureAwait(false);

        _logger.LogDebug("Additional data added to '{uri}'", uri);
    }
}