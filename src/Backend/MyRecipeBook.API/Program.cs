using MyRecipeBook.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddMyRecipeBookConfiguration();

builder.Services
    .AddMyRecipeBookSettings(builder.Configuration)
    .AddMyRecipeBookPresentation()
    .AddMyRecipeBookDocumentation()
    .AddMyRecipeBookDependencies(builder.Configuration)
    .AddMyRecipeBookAuthentication()
    .AddMyRecipeBookRateLimiting()
    .AddMyRecipeBookBackgroundServices()
    .AddMyRecipeBookHealthChecks();

var app = builder.Build();
var testEnvironmentSettings = app.GetTestEnvironmentSettings();

app.UseMyRecipeBookRequestPipeline(testEnvironmentSettings)
    .MapMyRecipeBookEndpoints();

app.MigrateMyRecipeBookDatabase(testEnvironmentSettings);

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}
