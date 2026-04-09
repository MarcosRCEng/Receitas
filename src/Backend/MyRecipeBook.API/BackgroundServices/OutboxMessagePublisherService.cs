using Microsoft.Extensions.Options;
using MyRecipeBook.Domain.Events;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.Outbox;
using MyRecipeBook.Domain.Services.ServiceBus;
using MyRecipeBook.Domain.Settings;
using MyRecipeBook.Domain.ValueObjects;
using System.Globalization;
using System.Text.Json;

namespace MyRecipeBook.API.BackgroundServices;

public class OutboxMessagePublisherService : BackgroundService
{
    private static readonly TimeSpan DelayBetweenRuns = TimeSpan.FromSeconds(10);
    private const int MAX_MESSAGES_PER_RUN = 10;

    private readonly IServiceProvider _services;
    private readonly AzureServiceBusSettings _serviceBusSettings;
    private readonly TestEnvironmentSettings _testEnvironmentSettings;

    public OutboxMessagePublisherService(
        IServiceProvider services,
        IOptions<AzureServiceBusSettings> serviceBusSettings,
        IOptions<TestEnvironmentSettings> testEnvironmentSettings)
    {
        _services = services;
        _serviceBusSettings = serviceBusSettings.Value;
        _testEnvironmentSettings = testEnvironmentSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_testEnvironmentSettings.InMemoryTest || _serviceBusSettings.IsConfigured().Equals(false))
            return;

        while (stoppingToken.IsCancellationRequested.Equals(false))
        {
            await PublishPendingMessages();

            await Task.Delay(DelayBetweenRuns, stoppingToken);
        }
    }

    private async Task PublishPendingMessages()
    {
        using var scope = _services.CreateScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var deleteUserQueue = scope.ServiceProvider.GetRequiredService<IDeleteUserQueue>();

        var messages = await outboxRepository.GetPending(MAX_MESSAGES_PER_RUN);

        foreach (var message in messages)
        {
            try
            {
                if (message.Type == OutboxMessageTypes.DELETE_USER_REQUESTED)
                {
                    var payload = JsonSerializer.Deserialize<DeleteUserRequestedEvent>(message.Payload)
                        ?? throw new InvalidOperationException("Invalid delete user outbox payload.");

                    await deleteUserQueue.SendMessage(
                        payload.UserIdentifier,
                        message.Id.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported outbox message type: {message.Type}");
                }

                var messageMarkedAsProcessed = await outboxRepository.MarkAsProcessed(message.Id);
                if (messageMarkedAsProcessed)
                    await unitOfWork.Commit();
            }
            catch (Exception exception)
            {
                var messageMarkedAsFailed = await outboxRepository.MarkAsFailed(message.Id, exception.Message);
                if (messageMarkedAsFailed)
                    await unitOfWork.Commit();
            }
        }
    }
}
