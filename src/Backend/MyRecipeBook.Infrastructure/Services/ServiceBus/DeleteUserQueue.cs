using Azure.Messaging.ServiceBus;
using MyRecipeBook.Domain.Services.ServiceBus;
using MyRecipeBook.Domain.ValueObjects;

namespace MyRecipeBook.Infrastructure.Services.ServiceBus;
public class DeleteUserQueue : IDeleteUserQueue
{
    private readonly ServiceBusSender _serviceBusSender;

    public DeleteUserQueue(ServiceBusSender serviceBusSender)
    {
        _serviceBusSender = serviceBusSender;
    }

    public async Task SendMessage(Guid userIdentifier, Guid? requestId = null, string? messageId = null)
    {
        var message = new ServiceBusMessage(userIdentifier.ToString())
        {
            Subject = OutboxMessageTypes.DELETE_USER_REQUESTED
        };

        if (string.IsNullOrWhiteSpace(messageId).Equals(false))
            message.MessageId = messageId;

        message.ApplicationProperties["UserIdentifier"] = userIdentifier.ToString();

        if (requestId.HasValue)
        {
            var normalizedRequestId = requestId.Value.ToString("N");

            message.CorrelationId = normalizedRequestId;
            message.ApplicationProperties["RequestId"] = normalizedRequestId;
        }

        await _serviceBusSender.SendMessageAsync(message);
    }
}
