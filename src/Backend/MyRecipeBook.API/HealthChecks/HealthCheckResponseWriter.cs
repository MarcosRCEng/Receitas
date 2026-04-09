using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace MyRecipeBook.API.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration,
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration = entry.Value.Duration,
                    error = entry.Value.Exception?.Message
                })
        };

        return httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
