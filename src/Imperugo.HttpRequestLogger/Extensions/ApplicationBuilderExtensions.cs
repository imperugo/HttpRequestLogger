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
    /// <param name="application"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseHttpLogger(this IApplicationBuilder application, Func<HttpContext, IHostEnvironment, bool>? options = null)
    {
        options ??= (context, host) => !host.IsProduction()
            && context.Request.Method != "OPTIONS"
            && !context.Request.Path.StartsWithSegments("/docs", StringComparison.OrdinalIgnoreCase)
            && !context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase)
            && !context.Request.Path.StartsWithSegments("/favicon", StringComparison.OrdinalIgnoreCase);

        application.UseMiddleware<HttpLoggerMiddleware>(options);

        return application;
    }
}
