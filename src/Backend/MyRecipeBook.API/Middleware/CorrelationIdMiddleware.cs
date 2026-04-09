using Serilog.Context;

namespace MyRecipeBook.API.Middleware;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetCorrelationId(context);

        context.TraceIdentifier = correlationId;
        context.Items[ItemKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }

    private void LogInvalidCorrelationId(string correlationId) =>
        _logger.LogWarning(
            "Ignoring invalid correlation id received in header {HeaderName}. Value: {CorrelationId}",
            HeaderName,
            correlationId);

    private string GetCorrelationId(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
            return Guid.NewGuid().ToString("N");

        if (correlationId.Length <= 128)
            return correlationId;

        LogInvalidCorrelationId(correlationId);

        return Guid.NewGuid().ToString("N");
    }
}
