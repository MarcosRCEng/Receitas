using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MyRecipeBook.API.HealthChecks;
using MyRecipeBook.API.Middleware;
using MyRecipeBook.Domain.Settings;
using MyRecipeBook.Infrastructure.Migrations;
using Serilog;
using Serilog.Events;

namespace MyRecipeBook.API.Extensions;

public static class WebApplicationExtensions
{
    public static TestEnvironmentSettings GetTestEnvironmentSettings(this WebApplication app) =>
        app.Services.GetRequiredService<IOptions<TestEnvironmentSettings>>().Value;

    public static WebApplication UseMyRecipeBookRequestPipeline(this WebApplication app, TestEnvironmentSettings testEnvironmentSettings)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            };

            options.GetLevel = (httpContext, _, exception) =>
            {
                if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest)
                    return LogEventLevel.Warning;

                return LogEventLevel.Information;
            };
        });

        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseMiddleware<ApiVersionHeaderMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<CultureMiddleware>();

        if (testEnvironmentSettings.InMemoryTest.Equals(false))
            app.UseHttpsRedirection();

        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapMyRecipeBookEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            AllowCachingResponses = false,
            ResponseWriter = HealthCheckResponseWriter.WriteAsync,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        app.MapControllers();

        return app;
    }

    public static void MigrateMyRecipeBookDatabase(this WebApplication app, TestEnvironmentSettings testEnvironmentSettings)
    {
        if (testEnvironmentSettings.InMemoryTest)
            return;

        var databaseSettings = app.Services.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        var databaseType = databaseSettings.GetDatabaseType();
        var connectionString = databaseSettings.GetConnectionString();

        var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        DatabaseMigration.Migrate(databaseType, connectionString, serviceScope.ServiceProvider);
    }
}
