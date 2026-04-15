using Serilog;

namespace MyRecipeBook.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddMyRecipeBookConfiguration(this WebApplicationBuilder builder)
    {
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

        return builder;
    }
}
