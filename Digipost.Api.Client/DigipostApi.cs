﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Digipost.Api.Client.Domain;

namespace Digipost.Api.Client
{
    public class DigipostClient
    {
        private ClientConfig ClientConfig { get; set; }
        private X509Certificate2 PrivateCertificate { get; set; }

        public DigipostClient(ClientConfig clientConfig, X509Certificate2 privateCertificate)
        {
            ClientConfig = clientConfig;
            this.PrivateCertificate = privateCertificate;
        }

        public async Task<string> Send(Message message)
        {
            const string uri = "messages";
            Logging.Log(TraceEventType.Information, "> Starting Send()");
            var loggingHandler = new LoggingHandler(new HttpClientHandler());
            var authenticationHandler = new AuthenticationHandler(ClientConfig, PrivateCertificate, uri, loggingHandler);
            Logging.Log(TraceEventType.Information, " - Initializing HttpClient");
            using (var client = new HttpClient(authenticationHandler))
            {
                client.Timeout = TimeSpan.FromMilliseconds(ClientConfig.TimeoutMilliseconds);
                client.BaseAddress = new Uri(ClientConfig.ApiUrl.AbsoluteUri);
                var boundary = Guid.NewGuid().ToString();

                Logging.Log(TraceEventType.Information, " - Initializing MultipartFormDataContent");
                using (var content = new MultipartFormDataContent(boundary))
                {
                    var mediaTypeHeaderValue = new MediaTypeHeaderValue("multipart/mixed");
                    mediaTypeHeaderValue.Parameters.Add(new NameValueWithParametersHeaderValue("boundary", boundary));
                    content.Headers.ContentType = mediaTypeHeaderValue;

                    {
                        Logging.Log(TraceEventType.Information, "  - Creating XML-body");
                        var xmlMessage = "";
                        xmlMessage = SerializeUtil.Serialize(message);
                        Logging.Log(TraceEventType.Information, String.Format("   -  XML-body \n [{0}]", xmlMessage));
                        var messageContent = new StringContent(xmlMessage);
                        messageContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.digipost-v6+xml");
                        messageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = "\"message\""
                        };
                        content.Add(messageContent);
                    }

                    {
                        Logging.Log(TraceEventType.Information, "  - Adding primary document");
                        var documentContent = new ByteArrayContent(message.PrimaryDocument.ContentBytes);
                        documentContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                        documentContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = message.PrimaryDocument.Guid
                        };
                        content.Add(documentContent);
                    }

                    {
                        foreach (Document attachment in message.Attachments)
                        {
                            Logging.Log(TraceEventType.Information, "  - Adding attachment");
                            var attachmentContent = new ByteArrayContent(attachment.ContentBytes);
                            attachmentContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                            attachmentContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = attachment.Guid
                            };
                            content.Add(attachmentContent);
                        }
                    }
                    
                    Logging.Log(TraceEventType.Information, String.Format(" - Posting to URL[{0}]", ClientConfig.ApiUrl+uri));
                    var requestResult = client.PostAsync(uri, content).Result;

                    var contentResult = await requestResult.Content.ReadAsStringAsync();
                    Logging.Log(TraceEventType.Information, String.Format("  - Result \n [{0}]", contentResult));

                    return contentResult;
                }
            }
            
        }
    }
}
