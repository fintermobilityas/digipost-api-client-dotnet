﻿using System;
using Digipost.Api.Client.Common.Enums;

namespace Digipost.Api.Client.Common.Identify;

public class IdentificationResult : IIdentificationResult
{
    public IdentificationResult(IdentificationResultType resultType, string resultCode = "")
    {
        ResultType = resultType;
        SetResultByIdentificationResultType(resultCode);
    }

    public IdentificationResultType ResultType { get; }

    public IdentificationError? Error { get; private set; }

    public string Data { get; set; }

    void SetResultByIdentificationResultType(string resultCode)
    {
        var allSuccessfulResultType = ResultType == IdentificationResultType.DigipostAddress ||
                                      ResultType == IdentificationResultType.Personalias;

        if (allSuccessfulResultType)
        {
            Data = resultCode;
        }
        else
        {
            Error = ParseToIdentificationError(resultCode);
        }
    }

    static IdentificationError ParseToIdentificationError(string identificationError)
    {
        switch (identificationError)
        {
            case "UNIDENTIFIED":
                return IdentificationError.Unidentified;
            case "INVALID":
                return IdentificationError.Invalid;
            case "INVALID_PERSONAL_IDENTIFICATION_NUMBER":
                return IdentificationError.InvalidPersonalIdentificationNumber;
            case "INVALID_ORGANISATION_NUMBER":
                return IdentificationError.InvalidOrganisationNumber;
            case "UNKNOWN":
                return IdentificationError.Unknown;
            case "MULTIPLE_MATCHES":
                return IdentificationError.MultipleMatches;
            case "NOT_FOUND":
                return IdentificationError.Unknown;
            default:
                throw new ArgumentOutOfRangeException(nameof(V8.Sensitivity_Level), identificationError, null);
        }
    }
}