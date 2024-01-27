using System.Collections.Generic;
using System.Threading;
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

internal class SendMessageApi(SendRequestHelper requestHelper, ILoggerFactory loggerFactory, Root root)
{
    const int MinimumSearchLength = 3;
    readonly ILogger<SendMessageApi> _logger = loggerFactory.CreateLogger<SendMessageApi>();

    public SendRequestHelper RequestHelper { get; } = requestHelper;

    public async Task<IMessageDeliveryResult> SendMessageAsync(IMessage message, bool skipMetaDataValidation = false, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Outgoing Digipost message to Recipient: {message}", message);

        var messageDelivery = await RequestHelper.PostMessageAsync<Message_Delivery>(message, root.GetSendMessageUri(), skipMetaDataValidation, cancellationToken);

        var messageDeliveryResult = messageDelivery.FromDataTransferObject();

        _logger.LogDebug("Response received for message to recipient, {message}: '{status}'. Will be available to Recipient at {deliverytime}.", message, messageDeliveryResult.Status, messageDeliveryResult.DeliveryTime);

        return messageDeliveryResult;
    }

    public async Task<IIdentificationResult> IdentifyAsync(IIdentification identification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Outgoing identification request: {identification}", identification);

        var identificationResultDataTransferObject = await RequestHelper.PostIdentificationAsync<Identification_Result>(identification, root.GetIdentifyRecipientUri(), cancellationToken);
        var identificationResult = identificationResultDataTransferObject.FromDataTransferObject();

        _logger.LogDebug("Response received for identification to recipient, ResultType '{resultType}', Data '{data}'.", identificationResult.ResultType, identificationResult.Data);

        return identificationResult;
    }

    public async Task<ISearchDetailsResult> SearchAsync(string search, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Outgoing search request, term: '{search}'.", search);

        search = search.RemoveReservedUriCharacters();
        var uri = root.GetRecipientSearchUri(search);

        if (search.Length < MinimumSearchLength)
        {
            var emptyResult = new SearchDetailsResult { PersonDetails = new List<SearchDetails>() };

            var taskSource = new TaskCompletionSource<ISearchDetailsResult>();
            taskSource.SetResult(emptyResult);
            return await taskSource.Task;
        }

        var searchDetailsResultDataTransferObject = await RequestHelper.GetAsync<Recipients>(uri, cancellationToken);

        var searchDetailsResult = searchDetailsResultDataTransferObject.FromDataTransferObject();

        _logger.LogDebug("Response received for search with term '{search}' retrieved.", search);

        return searchDetailsResult;
    }

    public async Task SendAdditionalDataAsync(IAdditionalData additionalData, AddAdditionalDataUri uri, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Sending additional data '{uri}'", uri);

        await RequestHelper.PostAdditionalDataAsync<string>(additionalData, uri, cancellationToken);

        _logger.LogDebug("Additional data added to '{uri}'", uri);
    }
}