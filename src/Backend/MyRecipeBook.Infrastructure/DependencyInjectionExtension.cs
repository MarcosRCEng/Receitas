using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyRecipeBook.Domain.Enums;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.Outbox;
using MyRecipeBook.Domain.Repositories.Recipe;
using MyRecipeBook.Domain.Repositories.Token;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Security.Cryptography;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Domain.Services.LoggedUser;
using MyRecipeBook.Domain.Services.OpenAI;
using MyRecipeBook.Domain.Services.ServiceBus;
using MyRecipeBook.Domain.Services.Storage;
using MyRecipeBook.Domain.Settings;
using MyRecipeBook.Domain.ValueObjects;
using MyRecipeBook.Infrastructure.DataAccess;
using MyRecipeBook.Infrastructure.DataAccess.Repositories;
using MyRecipeBook.Infrastructure.Security.Cryptography;
using MyRecipeBook.Infrastructure.Security.Tokens.Access.Generator;
using MyRecipeBook.Infrastructure.Security.Tokens.Access.Validator;
using MyRecipeBook.Infrastructure.Security.Tokens.Refresh;
using MyRecipeBook.Infrastructure.Services.LoggedUser;
using MyRecipeBook.Infrastructure.Services.OpenAI;
using MyRecipeBook.Infrastructure.Services.ServiceBus;
using MyRecipeBook.Infrastructure.Services.Storage;
using OpenAI.Chat;
using System.Reflection;

namespace MyRecipeBook.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        AddPasswordEncrpter(services);
        AddRepositories(services);
        AddLoggedUser(services);
        AddTokens(services);
        AddOpenAI(services);
        AddAzureStorage(services);
        AddQueue(services);

        if (IsUnitTestEnvironment(services))
            return;

        AddDbContext(services);
        AddFluentMigrator(services);
    }

    private static void AddDbContext(IServiceCollection services)
    {
        services.AddDbContext<MyRecipeBookDbContext>((serviceProvider, dbContextOptions) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            var connectionString = settings.GetConnectionString();
            var databaseType = settings.GetDatabaseType();

            if (databaseType == DatabaseType.PostgreSql)
            {
                dbContextOptions.UseNpgsql(connectionString);
                return;
            }

            if (databaseType == DatabaseType.MySql)
            {
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 35));
                dbContextOptions.UseMySql(connectionString, serverVersion);
                return;
            }

            dbContextOptions.UseSqlServer(connectionString);
        });
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserWriteOnlyRepository, UserRepository>();
        services.AddScoped<IUserReadOnlyRepository, UserRepository>();
        services.AddScoped<IUserUpdateOnlyRepository, UserRepository>();
        services.AddScoped<IUserDeleteOnlyRepository, UserRepository>();
        services.AddScoped<IRecipeWriteOnlyRepository, RecipeRepository>();
        services.AddScoped<IRecipeReadOnlyRepository, RecipeRepository>();
        services.AddScoped<IRecipeUpdateOnlyRepository, RecipeRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
    }


    private static void AddFluentMigrator(IServiceCollection services)
    {
        var databaseSettings = GetDatabaseSettings(services);
        var databaseType = databaseSettings.GetDatabaseType();

        if (databaseType == DatabaseType.MySql)
        {
            AddFluentMigrator_MySql(services);
            return;
        }

        if (databaseType == DatabaseType.SqlServer)
        {
            AddFluentMigrator_SqlServer(services);
            return;
        }

        AddFluentMigrator_PGSql(services);
    }

    private static void AddFluentMigrator_PGSql(IServiceCollection services)
    {
        services.AddFluentMigratorCore().ConfigureRunner(options =>
        {
            options
            .AddPostgres()
            .WithGlobalConnectionString(serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value.GetConnectionString())
            .ScanIn(Assembly.Load("MyRecipeBook.Infrastructure")).For.All();
        });
    }

    private static void AddFluentMigrator_MySql(IServiceCollection services)
    {
        services.AddFluentMigratorCore().ConfigureRunner(options =>
        {
            options
            .AddMySql5()
            .WithGlobalConnectionString(serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value.GetConnectionString())
            .ScanIn(Assembly.Load("MyRecipeBook.Infrastructure")).For.All();
        });
    }

    private static void AddFluentMigrator_SqlServer(IServiceCollection services)
    {
        services.AddFluentMigratorCore().ConfigureRunner(options =>
        {
            options
            .AddSqlServer()
            .WithGlobalConnectionString(serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value.GetConnectionString())
            .ScanIn(Assembly.Load("MyRecipeBook.Infrastructure")).For.All();
        });
    }

    private static void AddTokens(IServiceCollection services)
    {
        services.AddScoped<IAccessTokenGenerator>(option =>
        {
            var settings = option.GetRequiredService<IOptions<JwtSettings>>().Value;

            return new JwtTokenGenerator(
                settings.ExpirationTimeMinutes,
                settings.SigningKey,
                settings.Issuer,
                settings.Audience);
        });

        services.AddScoped<IAccessTokenValidator>(option =>
        {
            var settings = option.GetRequiredService<IOptions<JwtSettings>>().Value;

            return new JwtTokenValidator(
                settings.SigningKey,
                settings.Issuer,
                settings.Audience);
        });

        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
    }

    private static void AddLoggedUser(IServiceCollection services) => services.AddScoped<ILoggedUser, LoggedUser>();

    private static void AddPasswordEncrpter(IServiceCollection services)
    {
        services.AddScoped<IPasswordEncripter, BCryptNet>();
    }

    private static void AddOpenAI(IServiceCollection services)
    {
        services.AddScoped<IGenerateRecipeAI, ChatGptService>();

        services.AddScoped(c =>
        {
            var settings = c.GetRequiredService<IOptions<OpenAISettings>>().Value;

            return new ChatClient(MyRecipeBookRuleConstants.CHAT_MODEL, settings.ApiKey);
        });
    }

    private static void AddAzureStorage(IServiceCollection services)
    {
        services.AddScoped<IBlobStorageService>(c =>
        {
            var settings = c.GetRequiredService<IOptions<BlobStorageSettings>>().Value;

            if (settings.IsConfigured())
                return new AzureStorageService(new BlobServiceClient(settings.Azure));

            return new FakeBlobStorageService();
        });
    }

    private static void AddQueue(IServiceCollection services)
    {
        services.AddSingleton(c =>
        {
            var settings = c.GetRequiredService<IOptions<AzureServiceBusSettings>>().Value;

            return new ServiceBusClient(settings.DeleteUserAccount, new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            });
        });

        services.AddSingleton(c =>
        {
            var settings = c.GetRequiredService<IOptions<AzureServiceBusSettings>>().Value;
            var client = c.GetRequiredService<ServiceBusClient>();

            return new DeleteUserProcessor(client.CreateProcessor(settings.QueueName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1
            }));
        });

        services.AddScoped<IDeleteUserQueue>(c =>
        {
            var settings = c.GetRequiredService<IOptions<AzureServiceBusSettings>>().Value;

            if (settings.IsConfigured().Equals(false))
                return new FakeDeleteUserQueue();

            var client = c.GetRequiredService<ServiceBusClient>();

            return new DeleteUserQueue(client.CreateSender(settings.QueueName));
        });
    }

    private static bool IsUnitTestEnvironment(IServiceCollection services)
    {
        using var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IOptions<TestEnvironmentSettings>>().Value.InMemoryTest;
    }

    private static DatabaseSettings GetDatabaseSettings(IServiceCollection services)
    {
        using var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    }
}
