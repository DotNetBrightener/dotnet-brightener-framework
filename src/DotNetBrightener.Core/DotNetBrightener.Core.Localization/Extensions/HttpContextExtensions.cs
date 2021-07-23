using System;
using System.Globalization;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtensions
    {
        private const string CurrentCultureRequestKey = "DNB::CurrentRequestCulture";

        internal static void StoreCurrentCulture(this HttpContext httpContext, CultureInfo currentCulture)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            httpContext.Items.Add(CurrentCultureRequestKey, currentCulture);
        }

        public static CultureInfo GetCurrentCulture(this HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue(CurrentCultureRequestKey, out object cultureObj) &&
                cultureObj is CultureInfo result)
            {
                return result;
            }

            return CultureInfo.CurrentUICulture;
        }
    }
}