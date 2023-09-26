// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Imperugo.HttpRequestToCurl.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Imperugo.HttpRequestLogger;

/// <summary>
/// The Http Logger Options
/// </summary>
public sealed class HttpLoggerOptions
{
    private static bool DefaultLogggingRule(HttpContext ctx, IHostEnvironment env)
    {
        // This mean log in production
        if (env.IsProduction())
            return false;

        // CORS stuff
        if (ctx.Request.Method == "OPTIONS")
            return false;

        // Swagger
        if(ctx.Request.Path.StartsWithSegments("/docs", StringComparison.OrdinalIgnoreCase) || ctx.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
            return false;

        // Prometheus
        if(ctx.Request.Path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase))
            return false;

        // Browser
        if(ctx.Request.Path.StartsWithSegments("/favicon", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    /// <summary>
    /// The function that specify the rule for the logging
    /// </summary>
    public Func<HttpContext, IHostEnvironment, bool> LoggingRules { get; set; } = DefaultLogggingRule;

    /// <summary>
    /// Options for generating the curl command
    /// </summary>
    public ToCurlOptions CurlOptions { get; set; } = ToCurlOptions.Bash;

    /// <summary>
    /// The maximum size in bytes of the in-memory &lt;see cref="System.Buffers.ArrayPool{Byte}"/&gt; used to buffer the
    /// /// stream. Larger request bodies are written to disk.
    /// </summary>
    public int BufferThreshold { get; set; } = 1024 * 30;

    /// <summary>
    /// The maximum size in bytes of the request body. An attempt to read beyond this limit will cause an
    /// </summary>
    public long? BufferLimit{ get; set; }
}
