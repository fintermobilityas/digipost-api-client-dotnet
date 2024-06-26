﻿using System;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Tests;
using Xunit;

namespace Digipost.Api.Client.Send.Tests;

public class MessageTests
{
    internal readonly Sender Sender = new(1010);

    public class ConstructorMethod : MessageTests
    {
        [Fact]
        public void ConstructWithRecipientAndPrimaryDocument()
        {
            //Arrange
            using var primaryDocument = DomainUtility.GetDocument();
            var message = new Message(Sender, DomainUtility.GetRecipientByDigipostId(), primaryDocument);

            //Act

            //Assert
            Assert.NotNull(message.PrimaryDocument);
        }

        [Fact]
        public void ConstructWithRecipientByIdAndPrintDetailsMethod()
        {
            //Arrange
            var recipient = DomainUtility.GetRecipientByDigipostId();
            using var document = DomainUtility.GetDocument();
            var printDetails = DomainUtility.GetPrintDetails();

            var message = new Message(Sender, recipient, document) { PrintDetails = printDetails };

            //Act

            //Assert
            Assert.Equal(recipient, message.DigipostRecipient);
            Assert.Equal(document, message.PrimaryDocument);
            Assert.Equal(printDetails, message.PrintDetails);
        }
    }

    public class DeliveryTimeSpecifiedMethod : MessageTests
    {
        [Fact]
        public void DeliveryTimeNotSpecifiedGivesFalse()
        {
            //Arrange
            using var primaryDocument = DomainUtility.GetDocument();
            var message = new Message(Sender, DomainUtility.GetRecipientByDigipostId(), primaryDocument);

            //Act

            //Assert
            Assert.False(message.DeliveryTimeSpecified);
        }

        [Fact]
        public void DeliveryTimeSpecifiedGivesTrue()
        {
            //Arrange
            using var primaryDocument = DomainUtility.GetDocument();
            var message = new Message(Sender, DomainUtility.GetRecipientByDigipostId(), primaryDocument)
            {
                DeliveryTime = DateTime.Today
            };

            //Act

            //Assert
            Assert.True(message.DeliveryTimeSpecified);
        }
    }
}