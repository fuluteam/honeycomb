


using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace HoneyComb.API.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetAppId(this HttpContext httpContext)
        {
            if (httpContext.Request.Headers.TryGetValue("x-app-id", out StringValues headerValue))
            {
                return headerValue;
            }
            else
            {
                throw new Exception("Can not found tenant id from headers");
            }
        }
        // public static string GetAppId(this HttpContext httpContext)
        // {
        //     var appId = httpContext.User?.FindFirst(c => c.Type == JwtClaimTypes.ClientId)?.Value;
        //     Guard.IsNotNull(appId, nameof(appId));
        //     return appId;
        // }
    }

}
