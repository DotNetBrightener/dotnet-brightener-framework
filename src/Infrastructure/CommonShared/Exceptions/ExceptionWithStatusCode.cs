﻿using System;
using System.Net;

namespace DotNetBrightener.WebApp.CommonShared.Exceptions;

public class ExceptionWithStatusCode : Exception
{
    public ExceptionWithStatusCode(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; private set; }
}