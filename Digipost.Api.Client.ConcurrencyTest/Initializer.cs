﻿using System;
using System.Threading.Tasks;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.ConcurrencyTest.Enums;
using Environment = Digipost.Api.Client.Common.Environment;

namespace Digipost.Api.Client.ConcurrencyTest
{
    public class Initializer
    {
        private const ProcessingType ProcessingType = Enums.ProcessingType.Parallel;
        private const RequestType RequestType = Enums.RequestType.Message;
        private const string Thumbprint = "29 7e 44 24 f2 8d ed 2c 9a a7 3d 9b 22 7c 73 48 f1 8a 1b 9b";
        private const long SenderId = 106824802; //"779052"; 
        private const int DegreeOfParallelism = 4;
        private const int NumberOfRequests = 100;
        private const int ThreadsActive = 4;

        public static void Run()
        {
            Console.WriteLine("Starting program ...");
            await Digipost(NumberOfRequests, ThreadsActive, ProcessingType);
        }

        private static async Task Digipost(int numberOfRequests, int connectionLimit, ProcessingType processingType)
        {
            Console.WriteLine("Starting to send digipost: {0}, with requests: {1}, poolcount: {2}", processingType,
                numberOfRequests, connectionLimit);

            var clientConfig = new ClientConfig(new Broker(SenderId), Environment.Production);

            switch (processingType)
            {
                case ProcessingType.Parallel:
                    await new DigipostParalell(numberOfRequests, connectionLimit, DegreeOfParallelism, clientConfig,
                        Thumbprint).RunAsync(RequestType);
                    break;
                case ProcessingType.Async:
                    await new DigipostAsync(numberOfRequests, connectionLimit, clientConfig, Thumbprint).RunAsync(RequestType);
                    break;
            }
        }
    }
}
