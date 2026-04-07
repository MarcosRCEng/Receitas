using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories.Outbox;

namespace MyRecipeBook.Infrastructure.DataAccess.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly MyRecipeBookDbContext _dbContext;

    public OutboxRepository(MyRecipeBookDbContext dbContext) => _dbContext = dbContext;

    public async Task Add(OutboxMessage message)
    {
        await _dbContext.OutboxMessages.AddAsync(message);
    }

    public async Task<IList<OutboxMessage>> GetPending(int maxMessages)
    {
        return await _dbContext
            .OutboxMessages
            .AsNoTracking()
            .Where(message => message.ProcessedOn == null)
            .OrderBy(message => message.CreatedOn)
            .Take(maxMessages)
            .ToListAsync();
    }

    public async Task MarkAsProcessed(long id)
    {
        var message = await _dbContext.OutboxMessages.FirstAsync(message => message.Id == id);

        message.ProcessedOn = DateTime.UtcNow;
        message.Error = null;
    }

    public async Task MarkAsFailed(long id, string error)
    {
        var message = await _dbContext.OutboxMessages.FirstAsync(message => message.Id == id);

        message.RetryCount++;
        message.Error = error;
    }
}
