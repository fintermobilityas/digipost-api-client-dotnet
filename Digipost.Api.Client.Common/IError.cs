﻿namespace Digipost.Api.Client.Common;

public interface IError
{
    string Errorcode { get; set; }

    string Errormessage { get; set; }

    string Errortype { get; set; }

    string ToString();
}