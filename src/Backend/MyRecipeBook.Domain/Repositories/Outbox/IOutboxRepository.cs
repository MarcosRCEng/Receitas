using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Repositories.Outbox;

public interface IOutboxRepository
{
    Task Add(OutboxMessage message);
    Task<IList<OutboxMessage>> GetPending(int maxMessages);
    Task<bool> MarkAsProcessed(long id);
    Task<bool> MarkAsFailed(long id, string error);
}
