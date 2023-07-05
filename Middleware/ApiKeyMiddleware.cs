using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LoggerPlugin.Middleware
{
    public class ApiKeyMiddleware
    {
        private const string ApiKeyHeaderName = "X-API-KEY";
        private readonly RequestDelegate _next;
        private readonly string? _apiKey;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
        {

            _next = next;
            _apiKey = config["Apikey"]; // store API Key in appsettings.json or use a secure secret manager
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.ToString() == "/Logging")
            {
                if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var receivedApiKey))
                {
                    context.Response.StatusCode = 401;
                    Console.WriteLine(context.Request.Path.ToString());
                    await context.Response.WriteAsync($"API Key was not provided. (Header name: 'X-API-KEY'){_apiKey}");
                }

                else if (receivedApiKey != _apiKey)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid API Key.");
                }

                else
                {

                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }


    }
}
