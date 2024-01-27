using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.ConcurrencyTest.Enums;
using Digipost.Api.Client.Send;

namespace Digipost.Api.Client.ConcurrencyTest
{
    internal class DigipostParalell(
        int numberOfRequests,
        int defaultConnectionLimit,
        int degreeOfParallelism,
        ClientConfig clientConfig,
        string thumbprint)
        : DigipostRunner(clientConfig, thumbprint, numberOfRequests)
    {
        public override async Task RunAsync(RequestType requestType)
        {
            Stopwatch.Start();
            ServicePointManager.DefaultConnectionLimit = defaultConnectionLimit;

            var messages = new List<IMessage>();
            while (RunsLeft() > 0)
            {
                messages.Add(GetMessage());
            }

            var options = new ParallelOptions {MaxDegreeOfParallelism = degreeOfParallelism};
            
            await Parallel.ForEachAsync(messages, options, async (_, _) =>
            {
                await SendAsync(Client, requestType);
            });

            Stopwatch.Stop();
            DisplayTestResults();
        }
    }
}
