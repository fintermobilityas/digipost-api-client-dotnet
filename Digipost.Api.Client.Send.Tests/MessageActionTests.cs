using System;
using Digipost.Api.Client.Common.Utilities;
using Digipost.Api.Client.DataTypes.Core;
using Digipost.Api.Client.Send.Actions;
using Digipost.Api.Client.Tests;
using Xunit;

namespace Digipost.Api.Client.Send.Tests;

public class MessageActionTests
{
    public class RequestContentBody
    {
        [Fact]
        public void ReturnsCorrectDataForMessage()
        {
            //Arrange
            using var document = DomainUtility.GetDocument();
            var message = DomainUtility.GetSimpleMessageWithRecipientById(document);

            //Act
            var action = new MessageAction(message);
            var content = action.RequestContent;

            //Assert
            var expected = SerializeUtil.Serialize(message.ToDataTransferObject());
            Assert.Equal(expected, content.InnerXml);
        }

        [Fact]
        public void SerializedXmlContainsDataType()
        {
            var externalLink = new ExternalLink(new Uri("https://digipost.no"));

            using var document = DomainUtility.GetDocument(externalLink);
            var message = DomainUtility.GetSimpleMessageWithRecipientById(document);

            var action = new MessageAction(message);
            var content = action.RequestContent;

            Assert.Contains("<externalLink", content.InnerXml);
        }
    }
}