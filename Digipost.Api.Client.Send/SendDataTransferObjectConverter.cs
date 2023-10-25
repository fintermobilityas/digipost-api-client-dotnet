﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Enums;
using Digipost.Api.Client.Common.Extensions;
using V8;

namespace Digipost.Api.Client.Send
{
    internal static class SendDataTransferObjectConverter
    {
        public static V8.Message ToDataTransferObject(IMessage message)
        {
            var primaryDocumentDataTransferObject = ToDataTransferObject(message.PrimaryDocument);

            var messageDto = new V8.Message
            {
                Sender_Id = message.Sender.Id,
                Sender_IdSpecified = true,
                Primary_Document = primaryDocumentDataTransferObject,
                Message_Id = message.Id
            };

            if (message is PrintMessage)
            {
                messageDto.Recipient = new Message_Recipient()
                {
                    Print_Details = message.PrintDetails.ToDataTransferObject()
                };
            }
            else
            {
                messageDto.Recipient = DataTransferObjectConverter.ToDataTransferObject(message.DigipostRecipient);
                messageDto.Recipient.Print_Details = message.PrintDetails.ToDataTransferObject();
            }

            foreach (var document in message.Attachments.Select(ToDataTransferObject))
            {
                messageDto.Attachment.Add(document);
            }

            if (message.DeliveryTimeSpecified)
            {
                messageDto.Delivery_Time = message.DeliveryTime.Value;
                messageDto.Delivery_TimeSpecified = true;
            }

            if (message.PrintIfUnreadAfterSpecified)
            {
                messageDto.Print_If_Unread = DataTransferObjectConverter.ToDataTransferObject(message.PrintIfUnread);
            }

            if (message.RequestForRegistrationSpecified)
            {
                messageDto.Request_For_Registration = message.RequestForRegistration.ToDataTransferObject();
            }

            return messageDto;
        }

        public static V8.Document ToDataTransferObject(IDocument document)
        {
            var documentDto = new V8.Document
            {
                Subject = document.Subject,
                File_Type = document.FileType,
                Authentication_Level = document.AuthenticationLevel.ToAuthenticationLevel(),
                Authentication_LevelSpecified = true,
                Sensitivity_Level = document.SensitivityLevel.ToSensitivityLevel(),
                Sensitivity_LevelSpecified = true,
                Sms_Notification = ToDataTransferObject(document.SmsNotification),
                Uuid = document.Guid
            };

            if (document.DataType != null)
            {
                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(document.DataType);
                documentDto.Data_Type = new Data_Type()
                {
                    Any = xmldoc.DocumentElement
                };
            }

            return documentDto;
        }

        public static Sms_Notification ToDataTransferObject(ISmsNotification smsNotification)
        {
            if (smsNotification == null)
                return null;

            var smsNotificationDto = new Sms_Notification();

            if (smsNotification.NotifyAtTimes.Count > 0)
            {
                var timesAsListedTimes = smsNotification.NotifyAtTimes.Select(dateTime => new Listed_Time {Time = dateTime, TimeSpecified = true});
                foreach (var timesAsListedTime in timesAsListedTimes)
                {
                    smsNotificationDto.At.Add(timesAsListedTime);
                }
            }

            if (smsNotification.NotifyAfterHours.Count > 0)
            {
                foreach (var i in smsNotification.NotifyAfterHours.ToArray())
                {
                    smsNotificationDto.After_Hours.Add(i);
                }
            }

            return smsNotificationDto;
        }

        public static IMessageDeliveryResult FromDataTransferObject(Message_Delivery messageDeliveryDto)
        {
            IMessageDeliveryResult messageDeliveryResult = new MessageDeliveryResult
            {
                MessageId = messageDeliveryDto.Message_Id,
                PrimaryDocument = FromDataTransferObject(messageDeliveryDto.Primary_Document),
                Attachments = messageDeliveryDto.Attachment?.Select(FromDataTransferObject).ToList(),
                DeliveryTime = messageDeliveryDto.Delivery_Time,
                DeliveryMethod = messageDeliveryDto.Delivery_Method.ToDeliveryMethod(),
                Status = messageDeliveryDto.Status.ToMessageStatus(),
                SenderId = messageDeliveryDto.Sender_Id
            };

            return messageDeliveryResult;
        }

        public static IDocument FromDataTransferObject(V8.Document documentDto)
        {
            return new Document(documentDto.Subject, documentDto.File_Type, documentDto.Authentication_Level.ToAuthenticationLevel(), documentDto.Sensitivity_Level.ToSensitivityLevel(), FromDataTransferObject(documentDto.Sms_Notification))
            {
                Guid = documentDto.Uuid,
                ContentHash = new ContentHash {HashAlgoritm = documentDto.Content_Hash.Hash_Algorithm, Value = documentDto.Content_Hash.Value}
            };
        }

        public static ISmsNotification FromDataTransferObject(Sms_Notification smsNotificationDto)
        {
            if (smsNotificationDto == null)
                return null;

            var smsNotification = new SmsNotification
            {
                NotifyAfterHours = smsNotificationDto.After_Hours?.ToList() ?? new List<int>(),
                NotifyAtTimes = smsNotificationDto.At?.Select(listedTime => listedTime.Time).ToList() ?? new List<DateTime>()
            };

            return smsNotification;
        }

        public static DocumentStatus FromDataTransferObject(Document_Status dto)
        {
            return new DocumentStatus(
                dto.Uuid,
                dto.Sender_Id,
                dto.Created,
                dto.Status.ToDeliveryStatus(),
                dto.ReadSpecified ? dto.Read.ToRead() : (DocumentStatus.Read?) null,
                dto.Channel.ToDeliveryMethod(),
                dto.Content_Hash,
                dto.DeliveredSpecified ? dto.Delivered : (DateTime?) null,
                dto.Is_Primary_DocumentSpecified ? dto.Is_Primary_Document : (bool?) null,
                dto.Content_Hash_AlgorithmSpecified ? dto.Content_Hash_Algorithm.ToHashAlgoritm() : (HashAlgoritm?) null
            );
        }

        private static DocumentStatus.DocumentDeliveryStatus ToDeliveryStatus(this Delivery_Status deliveryStatus)
        {
            switch (deliveryStatus)
            {
                case Delivery_Status.DELIVERED:
                    return DocumentStatus.DocumentDeliveryStatus.DELIVERED;
                case Delivery_Status.NOT_DELIVERED:
                    return DocumentStatus.DocumentDeliveryStatus.NOT_DELIVERED;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static DocumentStatus.Read ToRead(this Read read)
        {
            switch (read)
            {
                case Read.Y:
                    return DocumentStatus.Read.YES;
                case Read.N:
                    return DocumentStatus.Read.NO;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
