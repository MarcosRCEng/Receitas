using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyRecipeBook.Infrastructure.Configuration;

public static class ConfigurationBindingExtensions
{
    public static IServiceCollection ConfigureSettings<TSettings>(
        this IServiceCollection services,
        IConfiguration configuration,
        params string[] sectionNames)
        where TSettings : class, new()
    {
        services
            .AddOptions<TSettings>()
            .Configure(options => configuration.BindSettings(options, sectionNames));

        return services;
    }

    public static TSettings GetSettings<TSettings>(
        this IConfiguration configuration,
        params string[] sectionNames)
        where TSettings : class, new()
    {
        var settings = new TSettings();
        configuration.BindSettings(settings, sectionNames);
        return settings;
    }

    public static void BindSettings<TSettings>(
        this IConfiguration configuration,
        TSettings settings,
        params string[] sectionNames)
        where TSettings : class
    {
        foreach (var sectionName in sectionNames.Where(section => string.IsNullOrWhiteSpace(section).Equals(false)))
            configuration.GetSection(sectionName).Bind(settings);
    }
}
