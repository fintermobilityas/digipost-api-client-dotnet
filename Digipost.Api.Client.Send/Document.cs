using System;
using System.IO;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Enums;
using Digipost.Api.Client.DataTypes.Core;

namespace Digipost.Api.Client.Send;

public sealed class Document : RestLinkable, IDocument
{
    /// <param name="subject">The subject of the document.</param>
    /// <param name="fileType">The file type of the file (e.g pdf,txt.. ).</param>
    /// <param name="contentStream">The content of file as stream.</param>
    /// <param name="authenticationLevel">Required authentication level of the document. Default password.</param>
    /// <param name="sensitivityLevel">Sensitivity level of the document. Default normal.</param>
    /// <param name="smsNotification">Optional notification to receiver of message via SMS. </param>
    /// <param name="dataType">Optional metadata for enriching the document when viewed in Digipost</param>
    public Document(string subject, string fileType, Stream contentStream, AuthenticationLevel authenticationLevel = AuthenticationLevel.Password,
        SensitivityLevel sensitivityLevel = SensitivityLevel.Normal, ISmsNotification smsNotification = null, IDigipostDataType dataType = null)
    {
        Guid = System.Guid.NewGuid().ToString();
        Subject = subject;
        FileType = fileType;
        Stream = contentStream;
        AuthenticationLevel = authenticationLevel;
        SensitivityLevel = sensitivityLevel;
        SmsNotification = smsNotification;
        DataType = dataType;
    }
    
    /// <param name="subject">The subject of the document.</param>
    /// <param name="fileType">The mime type of the file. e.g pdf,txt..</param>
    /// <param name="authenticationLevel">Required authentication level of the document. Default password.</param>
    /// <param name="sensitivityLevel">Sensitivity level of the document. Default normal.</param>
    /// <param name="smsNotification">Optional notification to receiver of message via SMS. </param>
    /// <param name="dataType">Optional metadata for enriching the document when viewed in Digipost</param>
    internal Document(string subject, string fileType, AuthenticationLevel authenticationLevel = AuthenticationLevel.Password,
        SensitivityLevel sensitivityLevel = SensitivityLevel.Normal, ISmsNotification smsNotification = null, IDigipostDataType dataType = null)
        : this(subject, fileType, null, authenticationLevel, sensitivityLevel, smsNotification, dataType)
    {
    }

    public ContentHash ContentHash { get; internal set; }

    public string Guid { get; set; }

    public string Subject { get; set; }

    public string FileType { get; set; }

    public ISmsNotification SmsNotification { get; set; }

    public AuthenticationLevel AuthenticationLevel { get; set; }

    public SensitivityLevel SensitivityLevel { get; set; }

    public Stream Stream { get; set; }

    public IDigipostDataType DataType { get; set; }

    public void Dispose()
    {
        Stream?.Dispose();
        Stream = null;
    }
}