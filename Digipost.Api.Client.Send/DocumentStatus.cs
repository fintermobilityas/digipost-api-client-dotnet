using System;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Enums;

namespace Digipost.Api.Client.Send;

public class DocumentStatus(
    string guid,
    long senderId,
    DateTime created,
    DocumentStatus.DocumentDeliveryStatus documentDeliveryStatus,
    DocumentStatus.Read? read,
    DeliveryMethod deliveryMethod,
    string contentHash,
    DateTime? delivered,
    bool? isPrimaryDocument,
    HashAlgoritm? contentHashAlgoritm)
{
    public string Guid { get; } = guid;

    public Sender Sender { get; } = new(senderId);

    public DateTime Created { get; } = created;

    /**
     * If DeliveryStatus is NOT_DELIVERED, Delivered will not have a value
     */
    public DateTime? Delivered { get; } = delivered;

    public DocumentDeliveryStatus DeliveryStatus { get; } = documentDeliveryStatus;

    public Read? DocumentRead { get; } = read;

    public DeliveryMethod DeliveryMethod { get; } = deliveryMethod;

    public string ContentHash { get; } = contentHash;

    public HashAlgoritm? ContentHashAlgoritm { get; } = contentHashAlgoritm;

    /**
     * isPrimaryDocument has value only if you ask api are the actual sender asking for DocumentStatus.
     * If you are, then this will be true for the primary document else false.
     */
    public bool? IsPrimaryDocument { get; } = isPrimaryDocument;

    public enum DocumentDeliveryStatus
    {
        /**
         * The document has been delivered
         */
        DELIVERED,

        /**
         * The document is still being processed
         */
        NOT_DELIVERED
    }

    /**
     * Indicates whether the document is read or not
     */
    public enum Read
    {
        YES,
        NO
    }
}