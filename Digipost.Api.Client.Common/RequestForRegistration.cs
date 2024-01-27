using System;
using Digipost.Api.Client.Common.Print;

namespace Digipost.Api.Client.Common;

public class RequestForRegistration
{
    public RequestForRegistration(
        DateTime registrationDeadline,
        string phoneNumber,
        string emailAddress,
        IPrintDetails printDetails
    )
    {
        RegistrationDeadline = registrationDeadline;
        PhoneNumber = phoneNumber;
        EmailAddress = emailAddress;
        PrintDetails = printDetails;
    }

    public DateTime RegistrationDeadline { get; }

    public string PhoneNumber { get; }

    public string EmailAddress { get; }

    public IPrintDetails PrintDetails { get; }
}