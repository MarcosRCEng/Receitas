using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MyRecipeBook.Domain.Settings;

namespace MyRecipeBook.API.HealthChecks;

public class AzureServiceBusHealthCheck(
    IServiceProvider serviceProvider,
    IOptions<AzureServiceBusSettings> serviceBusSettings,
    IOptions<TestEnvironmentSettings> testEnvironmentSettings) : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly AzureServiceBusSettings _serviceBusSettings = serviceBusSettings.Value;
    private readonly TestEnvironmentSettings _testEnvironmentSettings = testEnvironmentSettings.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_serviceBusSettings.IsConfigured().Equals(false))
        {
            if (_testEnvironmentSettings.InMemoryTest)
                return HealthCheckResult.Healthy("Service Bus skipped in test environment.");

            return HealthCheckResult.Degraded("Service Bus is not configured.");
        }

        try
        {
            var serviceBusClient = _serviceProvider.GetRequiredService<ServiceBusClient>();

            await using var receiver = serviceBusClient.CreateReceiver(_serviceBusSettings.QueueName);

            await receiver.PeekMessageAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy($"Service Bus queue '{_serviceBusSettings.QueueName}' is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Service Bus is unavailable.", ex);
        }
    }
}
