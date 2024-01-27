using System;
using System.Threading;
using System.Threading.Tasks;
using Digipost.Api.Client.Common.Identify;
using Digipost.Api.Client.Common.Relations;
using Digipost.Api.Client.Common.Utilities;
using Digipost.Api.Client.Send.Actions;

namespace Digipost.Api.Client.Send;

internal class SendRequestHelper
{
    readonly RequestHelper _requestHelper;

    internal SendRequestHelper(RequestHelper requestHelper)
    {
        _requestHelper = requestHelper;
    }

    internal Task<T> GetAsync<T>(Uri uri, CancellationToken cancellationToken)
    {
        return _requestHelper.GetAsync<T>(uri, cancellationToken);
    }

    internal Task<T> PostMessageAsync<T>(IMessage message, Uri uri, bool skipMetaDataValidation = false, CancellationToken cancellationToken = default)
    {
        var messageAction = new MessageAction(message);
        var httpContent = messageAction.Content(message);

        return _requestHelper.PostAsync<T>(httpContent, messageAction.RequestContent, uri, skipMetaDataValidation, cancellationToken);
    }

    internal Task PostAdditionalDataAsync<T>(IAdditionalData additionalData, AddAdditionalDataUri uri, CancellationToken cancellationToken)
    {
        var action = new AddAdditionalDataAction(additionalData);
        var httpContent = action.Content(additionalData);

        return _requestHelper.PostAsync<T>(httpContent, action.RequestContent, uri, cancellationToken: cancellationToken);
    }

    internal Task<T> PostIdentificationAsync<T>(IIdentification identification, Uri uri, CancellationToken cancellationToken)
    {
        var messageAction = new IdentificationAction(identification);
        var httpContent = messageAction.Content(identification);

        return _requestHelper.PostAsync<T>(httpContent, messageAction.RequestContent, uri, cancellationToken: cancellationToken);
    }
}