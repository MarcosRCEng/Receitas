using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyRecipeBook.API.BackgroundServices;
using MyRecipeBook.API.Converters;
using MyRecipeBook.API.Filters;
using MyRecipeBook.API.HealthChecks;
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
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace MyRecipeBook.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyRecipeBookSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureSettings<JwtSettings>(configuration, JwtSettings.LegacySectionName, JwtSettings.SectionName);
        services.ConfigureSettings<AzureServiceBusSettings>(configuration, AzureServiceBusSettings.LegacySectionName, AzureServiceBusSettings.SectionName);
        services.ConfigureSettings<BlobStorageSettings>(configuration, BlobStorageSettings.LegacySectionName, BlobStorageSettings.SectionName);
        services.ConfigureSettings<DatabaseSettings>(configuration, DatabaseSettings.LegacySectionName, DatabaseSettings.SectionName);
        services.ConfigureSettings<GoogleSettings>(configuration, GoogleSettings.LegacySectionName, GoogleSettings.SectionName);
        services.ConfigureSettings<IdCryptographySettings>(configuration, IdCryptographySettings.LegacySectionName, IdCryptographySettings.SectionName);
        services.ConfigureSettings<OpenAISettings>(configuration, OpenAISettings.LegacySectionName, OpenAISettings.SectionName);
        services.Configure<TestEnvironmentSettings>(configuration);

        return services;
    }

    public static IServiceCollection AddMyRecipeBookPresentation(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new StringConverter()));

        services.AddEndpointsApiExplorer();
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenProvider, HttpContextTokenValue>();

        return services;
    }

    public static IServiceCollection AddMyRecipeBookDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<IdsFilter>();

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = JwtBearerDefaults.AuthenticationScheme
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        },
                        Scheme = "oauth2",
                        Name = JwtBearerDefaults.AuthenticationScheme,
                        In = ParameterLocation.Header
                    },
                    []
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddMyRecipeBookDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    public static IServiceCollection AddMyRecipeBookAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer()
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            options.SlidingExpiration = false;
        })
        .AddGoogle(googleOptions =>
        {
            googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtSettings) =>
            {
                var settings = jwtSettings.Value;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal is null || TokenClaimReader.TryGetUserIdentifier(context.Principal, out _).Equals(false))
                            context.Fail(ResourceMessagesException.INVALID_SESSION);

                        return Task.CompletedTask;
                    },
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

        services.AddOptions<GoogleOptions>(GoogleDefaults.AuthenticationScheme)
            .Configure<IOptions<GoogleSettings>>((options, googleSettings) =>
            {
                var settings = googleSettings.Value;

                options.ClientId = settings.ClientId;
                options.ClientSecret = settings.ClientSecret;
                options.SaveTokens = false;
            });

        return services;
    }

    public static IServiceCollection AddMyRecipeBookRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
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

        return services;
    }

    public static IServiceCollection AddMyRecipeBookBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<OutboxMessagePublisherService>();
        services.AddHostedService<DeleteUserService>();

        return services;
    }

    public static IServiceCollection AddMyRecipeBookHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<MyRecipeBookDbContext>("database")
            .AddCheck<AzureServiceBusHealthCheck>("service_bus")
            .AddCheck<BlobStorageHealthCheck>("blob_storage");

        return services;
    }

    private static string GetRateLimitPartitionKey(HttpContext httpContext)
    {
        var userIdentifier = TokenClaimReader.TryGetUserIdentifier(httpContext.User, out var parsedUserIdentifier)
            ? parsedUserIdentifier.ToString()
            : null;

        return userIdentifier
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";
    }
}
