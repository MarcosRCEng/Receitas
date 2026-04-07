using Moq;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories.Outbox;

namespace CommonTestUtilities.Repositories;

public class OutboxRepositoryBuilder
{
    private readonly Mock<IOutboxRepository> _repository;
    private readonly IList<OutboxMessage> _messages = [];

    public OutboxRepositoryBuilder()
    {
        _repository = new Mock<IOutboxRepository>();
        _repository
            .Setup(repository => repository.Add(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(_messages.Add);
    }

    public IList<OutboxMessage> Messages => _messages;

    public IOutboxRepository Build() => _repository.Object;
}
