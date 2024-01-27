using System.Net;
using System.Threading.Tasks;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.ConcurrencyTest.Enums;

namespace Digipost.Api.Client.ConcurrencyTest
{
    internal class DigipostAsync(
        int numberOfRequests,
        int defaultConnectionLimit,
        ClientConfig clientconfig,
        string thumbprint)
        : DigipostRunner(clientconfig, thumbprint, numberOfRequests)
    {
        public override async Task RunAsync(RequestType requestType)
        {
            Stopwatch.Start();
            ServicePointManager.DefaultConnectionLimit = defaultConnectionLimit;

            while (RunsLeft() > 0)
            {
                await SendAsync(Client, requestType);
            }

            Stopwatch.Stop();
            DisplayTestResults();
        }
    }
}
