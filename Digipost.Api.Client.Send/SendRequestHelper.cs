﻿using System;
using System.Threading.Tasks;
using Digipost.Api.Client.Common.Actions;
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

    internal Task<T> Get<T>(Uri uri)
    {
        return _requestHelper.GetAsync<T>(uri);
    }

    internal Task<T> PostMessage<T>(IMessage message, Uri uri, bool skipMetaDataValidation = false)
    {
        var messageAction = new MessageAction(message);
        var httpContent = messageAction.Content(message);

        return _requestHelper.PostAsync<T>(httpContent, messageAction.RequestContent, uri, skipMetaDataValidation);
    }

    internal Task PostAdditionalData<T>(IAdditionalData additionalData, AddAdditionalDataUri uri)
    {
        var action = new AddAdditionalDataAction(additionalData);
        var httpContent = action.Content(additionalData);

        return _requestHelper.PostAsync<T>(httpContent, action.RequestContent, uri);
    }

    internal Task<T> PostIdentification<T>(IIdentification identification, Uri uri)
    {
        var messageAction = new IdentificationAction(identification);
        var httpContent = messageAction.Content(identification);

        return _requestHelper.PostAsync<T>(httpContent, messageAction.RequestContent, uri);
    }
}