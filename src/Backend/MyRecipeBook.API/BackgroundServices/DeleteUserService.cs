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

    public DeleteUserService(IServiceProvider services, IOptions<AzureServiceBusSettings> serviceBusSettings)
    {
        _services = services;
        _serviceBusSettings = serviceBusSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_serviceBusSettings.IsConfigured().Equals(false))
            return;

        var processor = _services.GetRequiredService<DeleteUserProcessor>().GetProcessor();

        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ExceptionReceivedHandler;

        await processor.StartProcessingAsync(stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs eventArgs)
    {
        var message = eventArgs.Message.Body.ToString();

        var userIdentifier = Guid.Parse(message);

        var scope = _services.CreateScope();

        var deleteUserUseCase = scope.ServiceProvider.GetRequiredService<IDeleteUserAccountUseCase>();

        await deleteUserUseCase.Execute(userIdentifier);
    }

    private static Task ExceptionReceivedHandler(ProcessErrorEventArgs _) => Task.CompletedTask;
}
