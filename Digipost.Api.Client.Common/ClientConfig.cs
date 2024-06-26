﻿using System.Net;

namespace Digipost.Api.Client.Common;

/// <summary>
///     Contains configuration for sending digital post.
/// </summary>
public class ClientConfig
{
    /// <summary>
    ///     Client configuration used for setting up the client with settings.
    /// </summary>
    /// <param name="broker">The broker is the actual sender of the message.</param>
    /// <param name="environment">Environment to connect to for sending.</param>
    public ClientConfig(Broker broker, Environment environment)
    {
        Environment = environment;
        Broker = broker;
    }

    public Environment Environment { get; set; }

    /// <summary>
    ///     Defines the timeout for communication with Digipost API. Default is 30 seconds.
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 30000;

    /// <summary>
    ///     The technical sender of messages to Digipost, known as the broker. This value is obtained during registration of
    ///     the broker. If the broker and the sender of the letter are the same organization, this is also the id of the
    ///     sender.
    /// </summary>
    public Broker Broker { get; set; }

    /// <summary>
    ///     If set, the proxy will be used for all requests. Remember to set <see cref="Credential" /> as well.
    /// </summary>
    public WebProxy WebProxy { get; set; }

    /// <summary>
    ///     Will be used if both this and <see cref="WebProxy" /> is set.
    /// </summary>
    public NetworkCredential Credential { get; set; }

    /// <summary>
    ///     If set to true, all requests and responses are logged with log level DEBUG.
    /// </summary>
    public bool LogRequestAndResponse { get; set; }

    /// <summary>
    ///     If set to true, document metadata xml will not be validated.
    ///     Used to test sending documents as if you've not imported the DataTypes project.
    /// </summary>
    public bool SkipMetaDataValidation { get; set; }

    /// <summary>
    /// The sender of the message, i.e. what the receiver of the message sees as the sender of the message.
    /// If you are delivering a message on behalf of an organization with id 5555, set this property
    /// to 5555. If you are delivering on behalf of yourself, set this to your organization`s sender id.
    /// The id is created by Digipost.
    /// </summary>
    public Sender Sender => new(Broker.Id);
}