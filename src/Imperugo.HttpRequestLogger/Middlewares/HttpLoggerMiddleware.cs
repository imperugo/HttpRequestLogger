// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Text;

using Imperugo.HttpRequestLogger.Abstractions;
using Imperugo.HttpRequestLogger.Storage;
using Imperugo.HttpRequestToCurl;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IO;

namespace Imperugo.HttpRequestLogger.Middlewares;

/// <summary>
/// The HttpLogger middleware
/// </summary>
public class HttpLoggerMiddleware
{
    private readonly RequestDelegate next;
    private readonly IHostEnvironment hostEnvironment;
    private readonly HttpLoggerOptions loggerOptions;
    private readonly IHttpLoggerService httpLoggerService;
    private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpLoggerMiddleware"/> class.
    /// </summary>
    /// <param name="next"></param>
    /// <param name="loggerOptions"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="httpLoggerService"></param>
    public HttpLoggerMiddleware(RequestDelegate next, HttpLoggerOptions loggerOptions, IHostEnvironment hostEnvironment, IHttpLoggerService httpLoggerService)
    {
        this.next = next;
        this.hostEnvironment = hostEnvironment;
        this.httpLoggerService = httpLoggerService;
        recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();

        this.loggerOptions = loggerOptions;
    }

    /// <inheritdoc/>
#pragma warning disable RCS1046
    public async Task Invoke(HttpContext context)
#pragma warning restore RCS1046
    {
        if (!loggerOptions.LoggingRules(context, hostEnvironment))
        {
            await next(context);
            return;
        }

        var measureExecution = new Stopwatch();
        measureExecution.Start();

        var request = await GetRequestStorageAsync(context.Request);

        var storage = new HttpLoggerStorage(
            method: context.Request.Method,
            url: context.Request.GetDisplayUrl(),
            request: request
        )
        {
            Machine = Environment.MachineName,
            ClientIp = context.Connection.RemoteIpAddress?.ToString(),
            UserClaims = GetUserClaims(context)
        };

        // Copy a pointer to the original response body stream
        var originalBodyStream = context.Response.Body;

        // Create a new memory stream...
        var responseBody = recyclableMemoryStreamManager.GetStream();

        // ...and use that for the temporary response body
        context.Response.Body = responseBody;

        var exceptionExist = false;

        // Continue down the Middleware pipeline, eventually returning to this class
        try
        {
            await next(context);
        }
        catch (Exception exc)
        {
            storage.Exception = exc.ToString();
            exceptionExist = true;

            throw;
        }
        finally
        {
            int? statusCode = null;

            if (exceptionExist)
                statusCode = 500;

            // Format the response from the server
            storage.Response = await GetResponseStorageAsync(context.Response, statusCode);

            storage.cUrl = storage.Request?.ToCurl(loggerOptions.CurlOptions);

            // // Save log to datastore
            measureExecution.Stop();
            storage.ExecutionDuration = measureExecution.Elapsed.Milliseconds;

            await httpLoggerService.SaveAsync(storage, context.RequestAborted);

            if (!exceptionExist)
            {
                // Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
                await responseBody.CopyToAsync(originalBodyStream);
            }
            else
            {
                context.Response.Body = originalBodyStream;
            }

            await responseBody.DisposeAsync();
        }
    }

    private UserClaim[] GetUserClaims(HttpContext context)
    {
        if (context.User.Identity == null)
            return Array.Empty<UserClaim>();

        // Is better to put a minimum capacity higher than start with 0 and then resize
        // Probably 10 is a good starting point
        var result = new List<UserClaim>(10);

        foreach (var claim in context.User.Claims)
            result.Add(new UserClaim(claim.Type, claim.Value));

        return result.ToArray();
    }

    private async Task<HttpRequestStorage?> GetRequestStorageAsync(HttpRequest request)
    {
        if(loggerOptions.BufferLimit == null)
            request.EnableBuffering(loggerOptions.BufferThreshold);
        else
            request.EnableBuffering(bufferThreshold: loggerOptions.BufferThreshold, bufferLimit: loggerOptions.BufferLimit.Value);

        if (!request.Body.CanRead)
            return null;

        var reader = new StreamReader(request.Body, encoding: Encoding.UTF8);

        var headers = new HeaderStorage[request.Headers.Count];
        var count = 0;

        foreach (var (key, value) in request.Headers)
        {
            headers[count] = new HeaderStorage(key, value.ToString() ?? string.Empty);
            count++;
        }

        var body = await reader.ReadToEndAsync();

        var result = new HttpRequestStorage(
                request.Method,
                request.ContentType,
                request.GetDisplayUrl(),
                headers,
                request.Protocol,
                body == string.Empty ? null : body);

        // Rewind
        request.Body.Seek(0, SeekOrigin.Begin);

        return result;
    }

    private async Task<HttpResponseStorage?> GetResponseStorageAsync(HttpResponse httpResponse, int? statusCode = null)
    {
        if (!httpResponse.Body.CanRead)
            return null;

        // We need to read the response stream from the beginning...
        httpResponse.Body.Seek(0, SeekOrigin.Begin);

        var responseHeaders = new HeaderStorage[httpResponse.Headers.Count];
        var headerIndex = 0;

        foreach (var (key,value) in httpResponse.Headers)
        {
            responseHeaders[headerIndex] = new HeaderStorage(key, value.ToString());
            headerIndex++;
        }

        var responseBody = await new StreamReader(httpResponse.Body).ReadToEndAsync();

        var response = new HttpResponseStorage(responseHeaders, statusCode ?? httpResponse.StatusCode)
        {
            Payload = responseBody == string.Empty ? null : responseBody
        };

        // We need to reset the reader for the response so that the client can read it.
        httpResponse.Body.Seek(0, SeekOrigin.Begin);

        return response;
    }
}
