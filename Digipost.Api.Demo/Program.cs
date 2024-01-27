using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Digipost.Api.Client;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Enums;
using Digipost.Api.Client.Common.Recipient;
using Digipost.Api.Client.Extensions;
using Digipost.Api.Client.Send;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Environment = Digipost.Api.Client.Common.Environment;

var serviceCollection = new ServiceCollection();

serviceCollection.AddLogging(builder =>
{
    builder.AddConsole()
        .SetMinimumLevel(LogLevel.Debug);
});

const long brokerId = 0;
const string certificatePath = "mycert.p12";
const string certificatePassword = "mypassword";
const string digipostAddress = "my.digipost#id";

var clientConfig = new ClientConfig(new Broker(brokerId), Environment.Test);
using var x509Certificate2 = new X509Certificate2(certificatePath, certificatePassword);

serviceCollection.AddSingleTenantDigipostClient(clientConfig, x509Certificate2);
var serviceProvider = serviceCollection.BuildServiceProvider();
var digipostClient = serviceProvider.GetRequiredService<IDigipostClient>();

var recipientById = new RecipientById(IdentificationType.DigipostAddress, digipostAddress);
using var primaryDocument = new Document("Hoveddokument", "pdf",  File.OpenRead("Hoveddokument.pdf"))
{
    DataType = new ExternalLink(new Uri("https://www.youpark.no"))
};
using var attachmentDocument = new Document("Vedlegg", "pdf",  File.OpenRead("Vedlegg.pdf"));

var result = await digipostClient.SendMessageAsync(new Message(clientConfig.Sender, recipientById, primaryDocument)
{
    Attachments = [attachmentDocument],
});

Console.WriteLine($"Message id: {result.MessageId}. Delivery status: {result.Status}.");