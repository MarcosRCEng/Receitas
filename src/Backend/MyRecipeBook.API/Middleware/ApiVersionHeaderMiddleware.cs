namespace MyRecipeBook.API.Middleware;

public class ApiVersionHeaderMiddleware(RequestDelegate next)
{
    private const string ApiVersionHeaderName = "x-api-version";
    private const string ApiVersionPathPrefix = "/api/v";

    public async Task InvokeAsync(HttpContext context)
    {
        var requestPath = context.Request.Path.Value;

        if (string.IsNullOrWhiteSpace(requestPath) || IsAlreadyVersioned(requestPath))
        {
            await next(context);
            return;
        }

        if (TryGetApiVersionFromHeader(context.Request.Headers, out var version))
        {
            var normalizedPath = requestPath.StartsWith('/')
                ? requestPath
                : $"/{requestPath}";

            context.Request.Path = new PathString($"{ApiVersionPathPrefix}{version}{normalizedPath}");
        }

        await next(context);
    }

    private static bool IsAlreadyVersioned(string requestPath)
        => requestPath.StartsWith(ApiVersionPathPrefix, StringComparison.OrdinalIgnoreCase);

    private static bool TryGetApiVersionFromHeader(IHeaderDictionary headers, out string version)
    {
        version = string.Empty;

        if (!headers.TryGetValue(ApiVersionHeaderName, out var headerValue))
            return false;

        version = headerValue.ToString().Trim();

        if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            version = version[1..];

        return int.TryParse(version, out var parsedVersion) && parsedVersion > 0;
    }
}
