﻿namespace Digipost.Api.Client.Test.CompareObjects
{
    public interface IDifference
    {
        string WhatIsCompared { get; set; }

        object ExpectedValue { get; set; }

        string ActualValue { get; set; }

        string PropertyName { get; set; }
    }
}