using Azure.Messaging.ServiceBus;
using MyRecipeBook.Application.UseCases.User.Delete.Delete;
using Microsoft.Extensions.Options;
using MyRecipeBook.Domain.Settings;
using MyRecipeBook.Infrastructure.Services.ServiceBus;

namespace MyRecipeBook.API.BackgroundServices;

public class DeleteUserService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly AzureServiceBusSettings _serviceBusSettings;
    private readonly ILogger<DeleteUserService> _logger;
    private ServiceBusProcessor? _processor;

    public DeleteUserService(
        IServiceProvider services,
        IOptions<AzureServiceBusSettings> serviceBusSettings,
        ILogger<DeleteUserService> logger)
    {
        _services = services;
        _serviceBusSettings = serviceBusSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_serviceBusSettings.IsConfigured().Equals(false))
        {
            _logger.LogInformation("Delete user service skipped because Azure Service Bus is not configured.");
            return;
        }

        _processor = _services.GetRequiredService<DeleteUserProcessor>().GetProcessor();

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ExceptionReceivedHandler;

        _logger.LogInformation(
            "Delete user service started. QueueName: {QueueName}",
            _serviceBusSettings.QueueName);

        try
        {
            await _processor.StartProcessingAsync(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Delete user service stopping.");
        }
        finally
        {
            if (_processor is not null)
            {
                _processor.ProcessMessageAsync -= ProcessMessageAsync;
                _processor.ProcessErrorAsync -= ExceptionReceivedHandler;

                if (_processor.IsProcessing)
                    await _processor.StopProcessingAsync(CancellationToken.None);

                await _processor.DisposeAsync();
            }
        }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs eventArgs)
    {
        var message = eventArgs.Message;
        var body = message.Body.ToString();

        if (Guid.TryParse(body, out var userIdentifier).Equals(false))
        {
            _logger.LogError(
                "Invalid delete user message payload. MessageId: {MessageId}. Body: {Body}. CorrelationId: {CorrelationId}. DeliveryCount: {DeliveryCount}",
                message.MessageId,
                body,
                message.CorrelationId,
                message.DeliveryCount);

            throw new InvalidOperationException($"Invalid delete user message payload for message '{message.MessageId}'.");
        }

        var requestId = GetApplicationProperty(message, "RequestId") ?? message.CorrelationId;

        _logger.LogInformation(
            "Processing delete user message. MessageId: {MessageId}. UserIdentifier: {UserIdentifier}. RequestId: {RequestId}. DeliveryCount: {DeliveryCount}. SequenceNumber: {SequenceNumber}",
            message.MessageId,
            userIdentifier,
            requestId,
            message.DeliveryCount,
            message.SequenceNumber);

        using var scope = _services.CreateScope();

        var deleteUserUseCase = scope.ServiceProvider.GetRequiredService<IDeleteUserAccountUseCase>();

        await deleteUserUseCase.Execute(userIdentifier);

        _logger.LogInformation(
            "Delete user message processed successfully. MessageId: {MessageId}. UserIdentifier: {UserIdentifier}. RequestId: {RequestId}",
            message.MessageId,
            userIdentifier,
            requestId);
    }

    private Task ExceptionReceivedHandler(ProcessErrorEventArgs eventArgs)
    {
        _logger.LogError(
            eventArgs.Exception,
            "Service Bus processor error. EntityPath: {EntityPath}. ErrorSource: {ErrorSource}. FullyQualifiedNamespace: {FullyQualifiedNamespace}",
            eventArgs.EntityPath,
            eventArgs.ErrorSource,
            eventArgs.FullyQualifiedNamespace);

        return Task.CompletedTask;
    }

    private static string? GetApplicationProperty(ServiceBusReceivedMessage message, string propertyName)
    {
        if (message.ApplicationProperties.TryGetValue(propertyName, out var value).Equals(false))
            return null;

        return value?.ToString();
    }
}
