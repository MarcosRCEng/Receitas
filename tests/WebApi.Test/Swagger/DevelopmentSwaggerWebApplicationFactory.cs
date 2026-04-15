using CommonTestUtilities.BlobStorage;
using CommonTestUtilities.Dtos;
using CommonTestUtilities.OpenAI;
using CommonTestUtilities.Tokens;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyRecipeBook.Domain.Services.OpenAI;
using MyRecipeBook.Infrastructure.DataAccess;

namespace WebApi.Test.Swagger;

public class DevelopmentSwaggerWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"SwaggerInMemoryDb-{Guid.NewGuid():N}";
    private readonly MyRecipeBook.Domain.Dtos.GeneratedRecipeDto _generatedRecipe = GeneratedRecipeDtoBuilder.Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development")
            .ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["InMemoryTest"] = bool.TrueString,
                    ["Jwt:SigningKey"] = TestJwtSettings.SigningKey,
                    ["Jwt:ExpirationTimeMinutes"] = TestJwtSettings.ExpirationTimeMinutes.ToString(),
                    ["Jwt:Issuer"] = TestJwtSettings.Issuer,
                    ["Jwt:Audience"] = TestJwtSettings.Audience
                });
            })
            .ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MyRecipeBookDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.RemoveAll<MyRecipeBookDbContext>();

                var provider = services.AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

                var blobStorage = new BlobStorageServiceBuilder().Build();
                services.AddScoped(_ => blobStorage);

                services.RemoveAll<IGenerateRecipeAI>();
                services.AddScoped<IGenerateRecipeAI>(_ => GenerateRecipeAIBuilder.Build(_generatedRecipe));

                services.AddDbContext<MyRecipeBookDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                    options.UseInternalServiceProvider(provider);
                });
            });
    }
}
