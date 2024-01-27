﻿using System;

namespace Digipost.Api.Client.Common.Exceptions;

public class XmlParseException : Exception
{
    static readonly string XmlException = "Could not parse parse XML.";

    public XmlParseException(string xmlRawData)
        : base(XmlException)
    {
        XmlRawData = xmlRawData;
    }

    public XmlParseException(string message, Exception inner, string xmlRawData)
        : base(message, inner)
    {
        XmlRawData = xmlRawData;
    }

    public string XmlRawData { get; }
}