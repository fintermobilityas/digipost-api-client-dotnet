using System;

namespace Digipost.Api.Client.Common;

public class Environment
{
    Environment(Uri url)
    {
        Url = url;
    }

    public Uri Url { get; set; }

    public static Environment Production => new(new Uri("https://api.digipost.no/"));

    public static Environment NorskHelsenett => new(new Uri("https://api.nhn.digipost.no"));

    public static Environment DifiTest => new(new Uri("https://api.difitest.digipost.no/"));

    public static Environment Test => new(new Uri("https://api.test.digipost.no/"));

    internal static Environment Qa => new(new Uri("https://api.qa.digipost.no/"));

    internal static Environment Local => new(new Uri("http://localhost:8282/"));

    public override string ToString()
    {
        return $"Url: {Url}";
    }
}