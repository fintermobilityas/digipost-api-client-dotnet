using System;
using System.Threading;
using System.Threading.Tasks;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Exceptions;
using Digipost.Api.Client.Common.Relations;
using Digipost.Api.Client.Common.SenderInfo;
using Digipost.Api.Client.Common.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace Digipost.Api.Client.Api;

internal sealed class SenderInformationApi
{
    readonly IMemoryCache _entrypointCache;
    readonly RequestHelper _requestHelper;

    internal SenderInformationApi(IMemoryCache entrypointCache, RequestHelper requestHelper)
    {
        _entrypointCache = entrypointCache;
        _requestHelper = requestHelper;
    }

    public async Task<SenderInformation> GetSenderInformationAsync(SenderInformationUri senderInformationUri, CancellationToken cancellationToken)
    {
        var cacheKey = "senderOrganisation" + senderInformationUri;

        if (_entrypointCache.TryGetValue(cacheKey, out SenderInformation information)) return information;

        var result = await _requestHelper.GetAsync<V8.Sender_Information>(senderInformationUri, cancellationToken);
        var senderInformation = result.FromDataTransferObject();

        if (!senderInformation.IsValidSender)
        {
            throw new ConfigException($"Broker not authorized or sender does not exist. {senderInformation.SenderStatus}");
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            // Keep in cache for 5 minutes when in use, but max 1 hour.
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        return _entrypointCache.Set(cacheKey, senderInformation, cacheEntryOptions);
    }

}