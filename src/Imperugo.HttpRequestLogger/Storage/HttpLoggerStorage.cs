// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Imperugo.HttpRequestToCurl;

namespace Imperugo.HttpRequestLogger.Storage;

/// <summary>
/// The http request and response
/// </summary>
public record HttpLoggerStorage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpLoggerStorage" /> class.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="url"></param>
    /// <param name="request"></param>
    public HttpLoggerStorage(
        string method,
        string url,
        HttpRequestStorage? request)
    {
        Method = method;
        Url = url;
        Request = request;
    }

    /// <summary>
    /// The request Id.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The time needed to handle the request in ms.
    /// </summary>
    public int ExecutionDuration { get; set; }

    /// <summary>
    /// The date and time when the middleware start to handle the request.
    /// </summary>
    /// <remarks>
    /// The value is based on UTC.
    /// </remarks>
    public DateTime When { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The server name.
    /// </summary>
    public string? Machine { get; set; }

    /// <summary>
    /// The client IP Address
    /// </summary>
    public string? ClientIp { get; set; }

    /// <summary>
    /// The http verb of the request
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// The requested url.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The users claims in case there is an active authentication
    /// </summary>
    public UserClaim[] UserClaims { get; set; } = Array.Empty<UserClaim>();

    /// <summary>
    /// The http request
    /// </summary>
    public HttpRequestStorage? Request { get; set; }

    /// <summary>
    /// The http response
    /// </summary>
    public HttpResponseStorage? Response { get; set; }

    /// <summary>
    /// The exception throw during the execution
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// The cURL of the request
    /// </summary>
    public string? cUrl { get; set; }
}

/// <summary>
/// The http response
/// </summary>
public record HttpResponseStorage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseStorage" /> class.
    /// </summary>
    /// <param name="headers">The http response headers.</param>
    /// <param name="statusCode">The http response status code.</param>
    public HttpResponseStorage(HeaderStorage[] headers, int statusCode)
    {
        Headers = headers;
        StatusCode = statusCode;
    }

    /// <summary>
    /// The http response headers.
    /// </summary>
    public HeaderStorage[] Headers { get; set; }

    /// <summary>
    /// The http response body.
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// The http response status code.
    /// </summary>
    public int StatusCode { get; set; }
}

/// <summary>
/// The user claim
/// </summary>
public class UserClaim
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserClaim" /> class.
    /// </summary>
    /// <param name="key">The claim key.</param>
    /// <param name="value">The claim value.</param>
    public UserClaim(string key, string value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    /// The claim key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The claim value.
    /// </summary>
    public string Value { get; }
}
