using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyRecipeBook.API.BackgroundServices;
using MyRecipeBook.API.Converters;
using MyRecipeBook.API.Filters;
using MyRecipeBook.API.HealthChecks;
using MyRecipeBook.API.Middleware;
using MyRecipeBook.API.Responses;
using MyRecipeBook.API.Token;
using MyRecipeBook.Application;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Domain.Settings;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Infrastructure;
using MyRecipeBook.Infrastructure.Configuration;
using MyRecipeBook.Infrastructure.DataAccess;
using MyRecipeBook.Infrastructure.Migrations;
using Serilog;
using Serilog.Events;
using System.Text;
using System.Threading.RateLimiting;

const string AUTHENTICATION_TYPE = "Bearer";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "MyRecipeBook.API");
});

builder.Services.ConfigureSettings<JwtSettings>(builder.Configuration, JwtSettings.LegacySectionName, JwtSettings.SectionName);
builder.Services.ConfigureSettings<AzureServiceBusSettings>(builder.Configuration, AzureServiceBusSettings.LegacySectionName, AzureServiceBusSettings.SectionName);
builder.Services.ConfigureSettings<BlobStorageSettings>(builder.Configuration, BlobStorageSettings.LegacySectionName, BlobStorageSettings.SectionName);
builder.Services.ConfigureSettings<DatabaseSettings>(builder.Configuration, DatabaseSettings.LegacySectionName, DatabaseSettings.SectionName);
builder.Services.ConfigureSettings<GoogleSettings>(builder.Configuration, GoogleSettings.LegacySectionName, GoogleSettings.SectionName);
builder.Services.ConfigureSettings<IdCryptographySettings>(builder.Configuration, IdCryptographySettings.LegacySectionName, IdCryptographySettings.SectionName);
builder.Services.ConfigureSettings<OpenAISettings>(builder.Configuration, OpenAISettings.LegacySectionName, OpenAISettings.SectionName);
builder.Services.Configure<TestEnvironmentSettings>(builder.Configuration);

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new StringConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<IdsFilter>();

    options.AddSecurityDefinition(AUTHENTICATION_TYPE, new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = AUTHENTICATION_TYPE
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = AUTHENTICATION_TYPE
                },
                Scheme = "oauth2",
                Name = AUTHENTICATION_TYPE,
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer()
.AddCookie()
.AddGoogle(googleOptions =>
{
    googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((options, jwtSettings) =>
    {
        var settings = jwtSettings.Value;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();

                if (context.Response.HasStarted)
                    return Task.CompletedTask;

                var hasAuthorizationHeader = context.Request.Headers.Authorization.Count > 0;
                var tokenIsExpired = context.AuthenticateFailure is SecurityTokenExpiredException;
                var errorMessage = hasAuthorizationHeader
                    ? ResourceMessagesException.INVALID_SESSION
                    : ResourceMessagesException.NO_TOKEN;

                return ApiErrorResponseWriter.WriteAsync(
                    context.HttpContext,
                    System.Net.HttpStatusCode.Unauthorized,
                    errorMessage,
                    tokenIsExpired);
            },
            OnForbidden = context =>
            {
                if (context.Response.HasStarted)
                    return Task.CompletedTask;

                return ApiErrorResponseWriter.WriteAsync(
                    context.HttpContext,
                    System.Net.HttpStatusCode.Forbidden,
                    ResourceMessagesException.USER_WITHOUT_PERMISSION_ACCESS_RESOURCE);
            }
        };
    });

builder.Services.AddOptions<GoogleOptions>(GoogleDefaults.AuthenticationScheme)
    .Configure<IOptions<GoogleSettings>>((options, googleSettings) =>
    {
        var settings = googleSettings.Value;

        options.ClientId = settings.ClientId;
        options.ClientSecret = settings.ClientSecret;
    });


builder.Services.AddScoped<ITokenProvider, HttpContextTokenValue>();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddHttpContextAccessor();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetRateLimitPartitionKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("AuthEndpoints", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetRateLimitPartitionKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.HttpContext.Response.HasStarted)
            return;

        await ApiErrorResponseWriter.WriteAsync(
            context.HttpContext,
            System.Net.HttpStatusCode.TooManyRequests,
            ResourceMessagesException.TOO_MANY_REQUESTS,
            cancellationToken: cancellationToken);
    };
});

builder.Services.AddHostedService<OutboxMessagePublisherService>();
builder.Services.AddHostedService<DeleteUserService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<MyRecipeBookDbContext>("database")
    .AddCheck<AzureServiceBusHealthCheck>("service_bus")
    .AddCheck<BlobStorageHealthCheck>("blob_storage");

var app = builder.Build();
var testEnvironmentSettings = app.Services.GetRequiredService<IOptions<TestEnvironmentSettings>>().Value;

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

    options.GetLevel = (httpContext, elapsed, exception) =>
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

var healthCheckOptions = new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    AllowCachingResponses = false,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
};

app.MapHealthChecks("/health", healthCheckOptions);

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

app.MapControllers();

MigrateDatabase();

await app.RunAsync();

void MigrateDatabase()
{
    if (testEnvironmentSettings.InMemoryTest)
        return;

    var databaseSettings = app.Services.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    var databaseType = databaseSettings.GetDatabaseType();
    var connectionString = databaseSettings.GetConnectionString();

    var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

    DatabaseMigration.Migrate(databaseType, connectionString, serviceScope.ServiceProvider);
}

static string GetRateLimitPartitionKey(HttpContext httpContext)
{
    var userIdentifier = httpContext.User.Identity?.IsAuthenticated is true
        ? httpContext.User.Identity.Name
        : null;

    return userIdentifier
        ?? httpContext.Connection.RemoteIpAddress?.ToString()
        ?? "anonymous";
}

public partial class Program
{
    protected Program() { }
}
