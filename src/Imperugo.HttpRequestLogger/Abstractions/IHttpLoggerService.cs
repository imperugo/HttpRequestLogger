// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Imperugo.HttpRequestLogger.Storage;

namespace Imperugo.HttpRequestLogger.Abstractions;

/// <summary>
/// The contract for the service that will store the request
/// </summary>
public interface IHttpLoggerService
{
    /// <summary>
    /// Save the Http request / response.
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task SaveAsync(HttpLoggerStorage storage, CancellationToken cancellationToken = default);
}
