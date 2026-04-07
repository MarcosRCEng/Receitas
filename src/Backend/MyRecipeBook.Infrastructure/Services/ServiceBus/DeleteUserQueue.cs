using Azure.Messaging.ServiceBus;
using MyRecipeBook.Domain.Services.ServiceBus;

namespace MyRecipeBook.Infrastructure.Services.ServiceBus;
public class DeleteUserQueue : IDeleteUserQueue
{
    private readonly ServiceBusSender _serviceBusSender;

    public DeleteUserQueue(ServiceBusSender serviceBusSender)
    {
        _serviceBusSender = serviceBusSender;
    }

    public async Task SendMessage(Guid userIdentifier, string? messageId = null)
    {
        var message = new ServiceBusMessage(userIdentifier.ToString());

        if (string.IsNullOrWhiteSpace(messageId).Equals(false))
            message.MessageId = messageId;

        await _serviceBusSender.SendMessageAsync(message);
    }
}
