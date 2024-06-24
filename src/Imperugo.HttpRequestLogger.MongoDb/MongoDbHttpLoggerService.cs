// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Imperugo.HttpRequestLogger.Abstractions;

using MongoDB.Driver;

namespace Imperugo.HttpRequestLogger.MongoDb;

/// <summary>
/// The implementation of <see cref="IHttpLoggerService"/> that store request / responses into MongoDb.
/// </summary>
public sealed class MongoDbHttpLoggerService : IHttpLoggerService
{
    private readonly IMongoCollection<HttpLoggerStorage> collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDbHttpLoggerService"/> class.
    /// </summary>
    /// <param name="mongoClient"></param>
    /// <param name="configuration"></param>
    public MongoDbHttpLoggerService(IMongoClient mongoClient, MongoDbConfiguration configuration)
    {
        collection = mongoClient.GetDatabase(configuration.DatabaseName).GetCollection<HttpLoggerStorage>(configuration.CollectionName);
    }

    /// <inheritdoc/>
    public Task SaveAsync(HttpLoggerStorage storage, CancellationToken cancellationToken = default)
    {
        return collection.InsertOneAsync(storage, cancellationToken: cancellationToken);
    }
}
