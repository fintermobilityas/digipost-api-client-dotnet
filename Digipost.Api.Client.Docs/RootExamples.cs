using System.ComponentModel.Design;
using Digipost.Api.Client.Common.Relations;
using Digipost.Api.Client.DataTypes.Core;
using Sender = Digipost.Api.Client.Common.Sender;

#pragma warning disable 0169
#pragma warning disable 0649

namespace Digipost.Api.Client.Docs;

public class RootExamples
{
    static readonly DigipostClient client;
    static readonly Sender sender;

    void FetchDefaultRoot()
    {
        var root = client.GetRootAsync(new ApiRootUri());
    }

    void FetchSenderRoot()
    {
        var root = client.GetRootAsync(new ApiRootUri(new Sender(1234)));
    }
}