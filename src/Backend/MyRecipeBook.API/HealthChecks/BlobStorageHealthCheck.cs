using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MyRecipeBook.Domain.Settings;

namespace MyRecipeBook.API.HealthChecks;

public class BlobStorageHealthCheck(
    IOptions<BlobStorageSettings> blobStorageSettings,
    IOptions<TestEnvironmentSettings> testEnvironmentSettings) : IHealthCheck
{
    private readonly BlobStorageSettings _blobStorageSettings = blobStorageSettings.Value;
    private readonly TestEnvironmentSettings _testEnvironmentSettings = testEnvironmentSettings.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_blobStorageSettings.IsConfigured().Equals(false))
        {
            if (_testEnvironmentSettings.InMemoryTest)
                return HealthCheckResult.Healthy("Blob Storage skipped in test environment.");

            return HealthCheckResult.Degraded("Blob Storage is not configured.");
        }

        try
        {
            var blobServiceClient = new BlobServiceClient(_blobStorageSettings.Azure);

            await blobServiceClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("Blob Storage account is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Blob Storage is unavailable.", ex);
        }
    }
}
