using System;

namespace DotNetBrightener.MultiTenancy.Exceptions
{
    /// <summary>
    /// Exception to be thrown when the tenant cannot be identify from the current HTTP request
    /// </summary>
    public class TenantCannotIdentifyException : Exception
    {
        public TenantCannotIdentifyException() : base($"Cannot identify the tenant information from Http request")
        {

        }
    }
}