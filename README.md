# Imperugo.HttpRequestLogger

[![Nuget](https://img.shields.io/nuget/v/Imperugo.HttpRequestLogger?style=flat-square)](https://www.nuget.org/packages/Imperugo.HttpRequestLogger/)
[![Nuget](https://img.shields.io/nuget/vpre/Imperugo.HttpRequestLogger?style=flat-square)](https://www.nuget.org/packages/Imperugo.HttpRequestLogger/)
[![Nuget](https://img.shields.io/nuget/v/Imperugo.HttpRequestLogger.MongoDb?style=flat-square)](https://www.nuget.org/packages/Imperugo.HttpRequestLogger.MongoDb/)
[![Nuget](https://img.shields.io/nuget/vpre/Imperugo.HttpRequestLogger.MongoDb?style=flat-square)](https://www.nuget.org/packages/Imperugo.HttpRequestLogger.MongoDb/)
[![GitHub](https://img.shields.io/github/license/imperugo/HttpRequestLogger?style=flat-square)](https://github.com/imperugo/HttpRequestLogger/blob/main/LICENSE)


The idea of this library is to have a very simple and lightweight logger for all the http requests and responses.
How many time someone told you "I had an error doing something on the website" and you first answer is "Do you have the cURL of the request?"
Well, if the answer is yes, this is your package.
This package is going to log the HTTP Request, the HTTP Response, ExecutionTime, Url, Headers and Exceptions.

## Quickstart

### Installation

Add the NuGet Package to your project:

```bash
dotnet add package Imperugo.HttpRequestLogger.MongoDb
```

> For now there is only the provider for MongoDb but I'll be happy to share more providers. Please feel free to open a PR with your provider.

### Usage

The useage is absolutely easy, register the dependency, the middleware and you are ready.:

```csharp
builder.Services.AddMongoDbHttpLoggerService(x =>
{
    x.ConnectionString = "mongodb://mongo.imperugo.com:27017";
    x.DatabaseName = "MySuperHttpLogger";
    x.CollectionName = "HttpRequests";
});
```

```csharp
app.UseHttpLogger();
```

> In order to have also the logged user claims, place the middleware after the authentication middleware.

The output should look like this:

<img width="1459" alt="CleanShot 2022-12-05 at 22 15 45@2x" src="https://user-images.githubusercontent.com/758620/205744325-e5edc3cf-3e80-4958-98b7-5f7c61f828df.png">

```json
{
    "_id" : "e13f092d-4b1b-4859-9101-7c335c21b7fa",
    "ExecutionDuration" : 7,
    "When" : "2022-12-05T19:26:39.875+0000",
    "Machine" : "Ugos-MacBook-Pro",
    "ClientIp" : "::1",
    "Method" : "POST",
    "Url" : "http://localhost:5051/api/user",
    "UserClaims" : [

    ],
    "Request" : {
        "Method" : "POST",
        "ContentType" : "application/json",
        "Url" : "http://localhost:5051/api/user",
        "Headers" : [
            {
                "Key" : "Accept",
                "Value" : "*/*"
            },
            {
                "Key" : "Connection",
                "Value" : "keep-alive"
            },
            {
                "Key" : "Host",
                "Value" : "localhost:5051"
            },
            {
                "Key" : "User-Agent",
                "Value" : "PostmanRuntime/7.29.2"
            },
            {
                "Key" : "Accept-Encoding",
                "Value" : "gzip, deflate, br"
            },
            {
                "Key" : "Cache-Control",
                "Value" : "no-cache"
            },
            {
                "Key" : "Content-Type",
                "Value" : "application/json"
            },
            {
                "Key" : "Content-Length",
                "Value" : "60"
            },
            {
                "Key" : "Postman-Token",
                "Value" : "164ae8eb-ac4e-46b9-8bb1-4192ecf4e295"
            }
        ],
        "Payload" : "{\n    \"firstname\" : \"Matilde\",\n    \"lastname\" : \"Schulist\"\n}"
    },
    "Response" : {
        "Headers" : [

        ],
        "Payload" : "",
        "StatusCode" : 404
    },
    "Exception" : null,
    "cUrl" : "curl --location \ncurl --location --request POST 'http://localhost:5051/api/user'\n--header 'Accept: */*' \n--header 'Connection: keep-alive' \n--header 'Host: localhost:5051' \n--header 'User-Agent: PostmanRuntime/7.29.2' \n--header 'Accept-Encoding: gzip, deflate, br' \n--header 'Cache-Control: no-cache' \n--header 'Content-Type: application/json' \n--header 'Content-Length: 60' \n--header 'Postman-Token: 164ae8eb-ac4e-46b9-8bb1-4192ecf4e295' \n--header 'Content-Type: application/json' \n--data-raw '{\n    \"firstname\" : \"Matilde\",\n    \"lastname\" : \"Schulist\"\n}'\n"
}
```

Now you are able to:

- Search requests by URL
- Search requests by ServerName
- Search requests with a specific status code response
- Search requests with specific headers
- Search requests in a specific time range

Of course, you can cross-reference all types of data in order to refine the search as much as possible.

## MongoDb Index

I'm not providing indexes for this because the correct index to use is related to the query you do.
Probably the most common valid index could be this:

```json
{
    "When" : -1,
    "Url" : 1
}
```

## Common questions:

**Q.** Why the logger is working only on Development environment.

**A.** This is pretty normal. The logger has been created for testing / development environment, not for production. This for several reason first of all privacy, secondo performance

---

**Q.** Can I enable it in production or only for specific criteria?

**A.** Yes you can, below the code show how:

```csharp
app.UseHttpLogger(new HttpLoggerOptions()
{
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
});app.UseHttpLogger(new HttpLoggerOptions()
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
```

---

**Q.** Can I use a different database?

**A.** Right now only MongoDb is supported but you can easily implement you favorite database (open a PR and I'll be happy to maintain it)

To do that is enough to install only the core package 

```bash
dotnet add package Imperugo.HttpRequestLogger****
```

Create a class that implments `IHttpLoggerService` with your logic and register it to the dependency injection

```csharp
services.AddSingleton<IHttpLoggerService, MySuperClass>();
```

---

**Q.** How can I replicate the request?
**A.** Into the database there is also the cURL so you can easily use it.

## Sample

Take a look [here](https://github.com/imperugo/HttpRequestLogger/blob/main/sample/Imperugo.HttpRequestLogger.Sample)

## License

Imperugo.HttpRequestToCurl [MIT](https://github.com/imperugo/HttpRequestToCurl/blob/main/LICENSE) licensed.

### Contributing

Thanks to all the people who already contributed!

<a href="https://github.com/imperugo/HttpRequestLogger/graphs/contributors">
  <img src="https://contributors-img.web.app/image?repo=imperugo/HttpRequestLogger" />
</a>
