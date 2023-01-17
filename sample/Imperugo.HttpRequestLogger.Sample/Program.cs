using Imperugo.HttpRequestLogger;
using Imperugo.HttpRequestLogger.Extensions;
using Imperugo.HttpRequestToCurl.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMongoDbHttpLoggerService(x =>
{
    x.ConnectionString = "mongodb://mongo.imperugo.com:27017";
    x.DatabaseName = "MySuperHttpLogger";
    x.CollectionName = "HttpRequests";
});

var app = builder.Build();

app.UseHttpLogger(new HttpLoggerOptions()
{
    CurlOptions = ToCurlOptions.PowerShell,
    LoggingRules = (ctx, env) =>
    {
        // This mean log in production
        if (!env.IsProduction())
            return false;

        // This mean skip options request, swagger and favicon
        if (ctx.Request.Method == "OPTIONS"
            || !ctx.Request.Path.StartsWithSegments("/docs", StringComparison.OrdinalIgnoreCase)
            || !ctx.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase)
            || !ctx.Request.Path.StartsWithSegments("/favicon", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // this mean log only if the logged user is imperugo
        return ctx.User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value == "imperugo";
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();
