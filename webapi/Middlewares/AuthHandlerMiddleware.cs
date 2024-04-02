﻿using Microsoft.AspNetCore.Authorization;

namespace webapi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class AuthHandlerMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            var routeRequiresAuthorization = context.GetEndpoint()?.Metadata.GetMetadata<AuthorizeAttribute>() != null;

            await next(context);

            if (routeRequiresAuthorization && context.Response.StatusCode == 401)
                context.Response.Headers.Append("X-AUTH-REQUIRED", true.ToString());
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class AuthHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthHandlerMiddleware>();
        }
    }
}
