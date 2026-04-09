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

        var processor = _services.GetRequiredService<DeleteUserProcessor>().GetProcessor();

        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ExceptionReceivedHandler;

        _logger.LogInformation("Delete user service started.");

        await processor.StartProcessingAsync(stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs eventArgs)
    {
        var message = eventArgs.Message.Body.ToString();

        _logger.LogInformation(
            "Processing delete user message. MessageId: {MessageId}",
            eventArgs.Message.MessageId);

        var userIdentifier = Guid.Parse(message);

        using var scope = _services.CreateScope();

        var deleteUserUseCase = scope.ServiceProvider.GetRequiredService<IDeleteUserAccountUseCase>();

        await deleteUserUseCase.Execute(userIdentifier);

        _logger.LogInformation(
            "Delete user message processed successfully. MessageId: {MessageId}. UserIdentifier: {UserIdentifier}",
            eventArgs.Message.MessageId,
            userIdentifier);
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
}
