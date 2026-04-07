using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Services.ServiceBus;
public interface IDeleteUserQueue
{
    Task SendMessage(Guid userIdentifier, string? messageId = null);
}
