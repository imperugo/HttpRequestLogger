// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Imperugo.HttpRequestLogger.Middlewares;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Imperugo.HttpRequestLogger.Extensions;

/// <summary>
/// The extensions methods for <see cref="IApplicationBuilder"/>.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Register the <see cref="HttpLoggerMiddleware"/> to the pipeline.
    /// </summary>
    /// <param name="application">The web application.</param>
    /// <param name="loggerOptions">The logger options.</param>
    /// <returns></returns>
    public static IApplicationBuilder UseHttpLogger(this IApplicationBuilder application, HttpLoggerOptions? loggerOptions = null)
    {
        loggerOptions ??= new HttpLoggerOptions();

        // Skip gRPC requests https://github.com/dotnet/aspnetcore/issues/39317
        application.UseWhen(
            ctx => ctx.Request.ContentType != "application/grpc",
            builder => builder.UseMiddleware<HttpLoggerMiddleware>(loggerOptions)
        );

        return application;
    }
}
