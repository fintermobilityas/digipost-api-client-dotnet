﻿using System;
using Xunit;
using Environment = Digipost.Api.Client.Common.Environment;

namespace Digipost.Api.Client.Tests
{
    public class EnvironmentTests
    {
        [Fact]
        public void Can_Change_Url()
        {
            var env = Environment.DifiTest;
            env.Url = new Uri("http://api.newenvironment.digipost.no");
        }
    }
}