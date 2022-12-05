using Imperugo.HttpRequestLogger.Abstractions;
using Imperugo.HttpRequestLogger.MongoDb;

using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A set of extensions methods for MongoDbHttpLogger
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all needed services to the dependency injection
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="dbName">The database to use.</param>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddMongoDbHttpLoggerService(this IServiceCollection services, string dbName, string collectionName)
    {
        var configuration = new MongoDbConfiguration()
        {
            CollectionName = collectionName, DatabaseName = dbName
        };

        services.AddSingleton(configuration);
        services.AddSingleton<IHttpLoggerService, MongoDbHttpLoggerService>();

        return services;
    }

    /// <summary>
    /// Register all needed services to the dependency injection
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="opt">The MongoDb configuration.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddMongoDbHttpLoggerService(this IServiceCollection services, Action<MongoDbConfiguration> opt)
    {
        var configuration = new MongoDbConfiguration(string.Empty);

        opt.Invoke(configuration);

        IMongoClient client = new MongoClient(configuration.ConnectionString);

        services.AddSingleton(client);
        services.AddSingleton(configuration);
        services.AddSingleton<IHttpLoggerService, MongoDbHttpLoggerService>();

        return services;
    }
}
