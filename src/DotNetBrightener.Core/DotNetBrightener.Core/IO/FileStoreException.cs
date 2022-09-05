﻿using System;

namespace DotNetBrightener.Core.IO;

public class FileStoreException : InvalidOperationException
{
    public FileStoreException(string s): base(s)
    {
    }
    public FileStoreException(string s, Exception innerException): base(s, innerException)
    {
    }
}