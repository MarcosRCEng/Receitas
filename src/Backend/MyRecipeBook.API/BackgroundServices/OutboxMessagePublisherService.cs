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
    private readonly ILogger<OutboxMessagePublisherService> _logger;

    public OutboxMessagePublisherService(
        IServiceProvider services,
        IOptions<AzureServiceBusSettings> serviceBusSettings,
        IOptions<TestEnvironmentSettings> testEnvironmentSettings,
        ILogger<OutboxMessagePublisherService> logger)
    {
        _services = services;
        _serviceBusSettings = serviceBusSettings.Value;
        _testEnvironmentSettings = testEnvironmentSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_testEnvironmentSettings.InMemoryTest || _serviceBusSettings.IsConfigured().Equals(false))
        {
            _logger.LogInformation(
                "Outbox publisher service skipped. InMemoryTest: {InMemoryTest}. ServiceBusConfigured: {ServiceBusConfigured}",
                _testEnvironmentSettings.InMemoryTest,
                _serviceBusSettings.IsConfigured());
            return;
        }

        _logger.LogInformation(
            "Outbox publisher service started. QueueName: {QueueName}. DelayBetweenRunsSeconds: {DelayBetweenRunsSeconds}. MaxMessagesPerRun: {MaxMessagesPerRun}",
            _serviceBusSettings.QueueName,
            DelayBetweenRuns.TotalSeconds,
            MAX_MESSAGES_PER_RUN);

        try
        {
            while (stoppingToken.IsCancellationRequested.Equals(false))
            {
                await PublishPendingMessages(stoppingToken);

                await Task.Delay(DelayBetweenRuns, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Outbox publisher service stopping.");
        }
    }

    private async Task PublishPendingMessages(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var deleteUserQueue = scope.ServiceProvider.GetRequiredService<IDeleteUserQueue>();

        var messages = await outboxRepository.GetPending(MAX_MESSAGES_PER_RUN);

        if (messages.Count > 0)
        {
            _logger.LogInformation(
                "Publishing pending outbox messages. Count: {MessageCount}",
                messages.Count);
        }

        foreach (var message in messages)
        {
            stoppingToken.ThrowIfCancellationRequested();

            try
            {
                if (message.Type == OutboxMessageTypes.DELETE_USER_REQUESTED)
                {
                    var payload = DeserializeDeleteUserRequestedEvent(message);

                    _logger.LogInformation(
                        "Publishing delete user outbox message. MessageId: {MessageId}. MessageType: {MessageType}. UserIdentifier: {UserIdentifier}. RequestId: {RequestId}. RetryCount: {RetryCount}. RequestedOnUtc: {RequestedOnUtc}",
                        message.Id,
                        message.Type,
                        payload.UserIdentifier,
                        payload.RequestId,
                        message.RetryCount,
                        payload.RequestedOnUtc);

                    await deleteUserQueue.SendMessage(
                        payload.UserIdentifier,
                        payload.RequestId,
                        message.Id.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported outbox message type: {message.Type}");
                }

                var messageMarkedAsProcessed = await outboxRepository.MarkAsProcessed(message.Id);
                if (messageMarkedAsProcessed)
                {
                    await unitOfWork.Commit();
                    _logger.LogInformation(
                        "Outbox message processed successfully. MessageId: {MessageId}. MessageType: {MessageType}",
                        message.Id,
                        message.Type);
                }
                else
                {
                    _logger.LogWarning(
                        "Outbox message could not be marked as processed because it was not found. MessageId: {MessageId}. MessageType: {MessageType}",
                        message.Id,
                        message.Type);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Error processing outbox message. MessageId: {MessageId}. MessageType: {MessageType}. RetryCount: {RetryCount}",
                    message.Id,
                    message.Type,
                    message.RetryCount);

                var messageMarkedAsFailed = await outboxRepository.MarkAsFailed(message.Id, GetFailureReason(exception));
                if (messageMarkedAsFailed)
                {
                    await unitOfWork.Commit();
                    _logger.LogWarning(
                        "Outbox message marked as failed. MessageId: {MessageId}. MessageType: {MessageType}. NextRetryCount: {NextRetryCount}",
                        message.Id,
                        message.Type,
                        message.RetryCount + 1);
                }
                else
                {
                    _logger.LogWarning(
                        "Outbox message could not be marked as failed because it was not found. MessageId: {MessageId}. MessageType: {MessageType}",
                        message.Id,
                        message.Type);
                }
            }
        }
    }

    private static DeleteUserRequestedEvent DeserializeDeleteUserRequestedEvent(Domain.Entities.OutboxMessage message)
    {
        var payload = JsonSerializer.Deserialize<DeleteUserRequestedEvent>(message.Payload)
            ?? throw new InvalidOperationException("Invalid delete user outbox payload.");

        if (payload.UserIdentifier == Guid.Empty)
            throw new InvalidOperationException("Delete user outbox payload is missing the user identifier.");

        return payload;
    }

    private static string GetFailureReason(Exception exception)
    {
        var error = exception.GetBaseException().Message;

        return string.IsNullOrWhiteSpace(error)
            ? exception.GetType().Name
            : error;
    }
}
