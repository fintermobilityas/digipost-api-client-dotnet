﻿using System;
using System.IO;
using Digipost.Api.Client.Common;
using Digipost.Api.Client.Common.Enums;

namespace Digipost.Api.Client.Send.Tests.Fakes
{
    internal class FakeInvoice : Invoice
    {
        public FakeInvoice(string subject, string fileType, byte[] contentBytes, decimal amount, string account, DateTime duedate, string kid = null, AuthenticationLevel authenticationLevel = AuthenticationLevel.Password, SensitivityLevel sensitivityLevel = SensitivityLevel.Normal, ISmsNotification smsNotification = null)
            : base(subject, fileType, contentBytes, amount, account, duedate, kid, authenticationLevel, sensitivityLevel, smsNotification)
        {
        }

        public FakeInvoice(string subject, string fileType, string path, decimal amount, string account, DateTime duedate, string kid = null, AuthenticationLevel authenticationLevel = AuthenticationLevel.Password, SensitivityLevel sensitivityLevel = SensitivityLevel.Normal, ISmsNotification smsNotification = null)
            : base(subject, fileType, path, amount, account, duedate, kid, authenticationLevel, sensitivityLevel, smsNotification)
        {
        }

        public FakeInvoice(string subject, string fileType, Stream contentStream, decimal amount, string account, DateTime duedate, string kid = null, AuthenticationLevel authenticationLevel = AuthenticationLevel.Password, SensitivityLevel sensitivityLevel = SensitivityLevel.Normal, ISmsNotification smsNotification = null)
            : base(subject, fileType, contentStream, amount, account, duedate, kid, authenticationLevel, sensitivityLevel, smsNotification)
        {
        }

        internal override byte[] ReadAllBytes(Stream documentStream)
        {
            return new byte[] {1, 2, 3};
        }

        internal override byte[] ReadAllBytes(string pathToDocument)
        {
            return new byte[] {1, 2, 3, 4};
        }
    }
}