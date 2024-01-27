using System;
using System.IO;
using Digipost.Api.Client.Common.Enums;
using Digipost.Api.Client.Resources.Content;
using Xunit;

namespace Digipost.Api.Client.Send.Tests;

public class DocumentTests
{
    public class ConstructorMethod : DocumentTests
    {
        [Fact]
        public void DocumentFromStream()
        {
            //Arrange
            using var contentStream = new MemoryStream(ContentResource.Hoveddokument.Pdf());

            //Act
            using var document = new Document("Subject", "txt", contentStream, AuthenticationLevel.TwoFactor, SensitivityLevel.Sensitive, new SmsNotification(2));

            //Assert
            Assert.NotNull(document.Guid);
            Assert.Equal("Subject", document.Subject);
            Assert.Equal("txt", document.FileType);
            var expectedBytes = ContentResource.Hoveddokument.Pdf();
            Assert.Equal(expectedBytes, BytesFromStream(document.Stream));
            Assert.Equal(AuthenticationLevel.TwoFactor, document.AuthenticationLevel);
            Assert.Equal(SensitivityLevel.Sensitive, document.SensitivityLevel);
            //Assert.Equal(new SmsNotification(2), document.SmsNotification);
        }

        static Span<byte> BytesFromStream(Stream fileStream)
        {
            if (fileStream == null) return null;
            var bytes = new byte[fileStream.Length];
            var bytesRead = fileStream.Read(bytes, 0, bytes.Length);
            return bytes.AsSpan(0, bytesRead);
        }
    }
}