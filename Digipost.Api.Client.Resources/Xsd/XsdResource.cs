﻿using System.IO;
using System.Reflection;
using System.Xml;
using Digipost.Api.Client.Shared.Resources.Resource;

namespace Digipost.Api.Client.Resources.Xsd;

internal static class XsdResource
{
    static readonly ResourceUtility ApiResourceUtility = new(typeof(XsdResource).Assembly, "Digipost.Api.Client.Resources.Xsd.Data");

    public static XmlReader GetApiV8Xsd()
    {
        return XmlReader.Create(new MemoryStream(ApiResourceUtility.ReadAllBytes("api_v8.xsd")));
    }

    public static XmlReader GetDataTypesXsd(Assembly assembly)
    {
        return XmlReader.Create(new MemoryStream(new ResourceUtility(assembly, "Digipost.Api.Client.DataTypes.Core.Resources.XSD").ReadAllBytes("datatypes.xsd")));
    }
}