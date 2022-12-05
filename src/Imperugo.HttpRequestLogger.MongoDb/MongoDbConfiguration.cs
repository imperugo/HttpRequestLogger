// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace Imperugo.HttpRequestLogger.MongoDb;

/// <summary>
/// The MongoDb configuration
/// </summary>
public sealed class MongoDbConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDbConfiguration"/> class.
    /// </summary>
    public MongoDbConfiguration()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDbConfiguration"/> class.
    /// </summary>
    /// <param name="connectionString"></param>
    public MongoDbConfiguration(string connectionString)
    {
        ConnectionString = connectionString;
        DatabaseName = "HttpLogger";
        CollectionName = "HttpRequests";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDbConfiguration"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="collectionName">The collection name.</param>
    public MongoDbConfiguration(string connectionString, string databaseName, string collectionName)
    {
        ConnectionString = connectionString;
        DatabaseName = databaseName;
        CollectionName = collectionName;
    }

    /// <summary>
    /// The connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// The database name.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// The collection name.
    /// </summary>
    public string? CollectionName { get; set; }
}
