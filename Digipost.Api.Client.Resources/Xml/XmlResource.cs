﻿using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Digipost.Api.Client.Shared.Resources.Resource;

namespace Digipost.Api.Client.Resources.Xml;

internal static class XmlResource
{
    static readonly ResourceUtility ResourceUtility = new(typeof(XmlResource).GetTypeInfo().Assembly, "Digipost.Api.Client.Resources.Xml.Data");

    static StringContent GetResource(params string[] path)
    {
        var bytes = ResourceUtility.ReadAllBytes(path);

        if (bytes == null)
        {
            throw new FileLoadException($"Unable to load file at {string.Join("/", path)}. Remember to add file as Resource. Open Properties on file in Solution Explorer (Alt + Enter), and set Build Action to Embedded resource.");
        }

        return new StringContent(XmlUtility.ToXmlDocument(Encoding.UTF8.GetString(bytes)).OuterXml);
    }

    internal static class SendMessage
    {
        public static StringContent GetError()
        {
            return GetResource("SendMessageErrorUnknownRecipient.xml");
        }

        public static StringContent GetMessageDelivery()
        {
            return GetResource("SendMessageMessageDelivery.xml");
        }
    }

    internal static class Identification
    {
        public static StringContent GetResult()
        {
            return GetResource("IdentificationResult.xml");
        }
    }

    internal static class Search
    {
        public static StringContent GetResult()
        {
            return GetResource("SearchResult.xml");
        }
    }

    internal static class Inbox
    {
        public static StringContent GetError()
        {
            return GetResource("InboxDocumentNotExisting.xml");
        }
    }
}