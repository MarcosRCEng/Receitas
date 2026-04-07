using CommonTestUtilities.Entities;
using CommonTestUtilities.LoggedUser;
using CommonTestUtilities.Repositories;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.User.Delete.Request;
using MyRecipeBook.Domain.Events;
using MyRecipeBook.Domain.ValueObjects;
using System.Text.Json;
using Xunit;

namespace UseCases.Test.User.Delete.Request;
public class RequestDeleteUserUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        (var user, _) = UserBuilder.Build();

        var outboxRepository = new OutboxRepositoryBuilder();

        var useCase = CreateUseCase(user, outboxRepository);

        var act = async () => await useCase.Execute();

        await act.Should().NotThrowAsync();

        user.Active.Should().BeFalse();
        outboxRepository.Messages.Should().ContainSingle();

        var message = outboxRepository.Messages.Single();
        message.Type.Should().Be(OutboxMessageTypes.DELETE_USER_REQUESTED);

        var payload = JsonSerializer.Deserialize<DeleteUserRequestedEvent>(message.Payload);
        payload!.UserIdentifier.Should().Be(user.UserIdentifier);
    }

    private static RequestDeleteUserUseCase CreateUseCase(
        MyRecipeBook.Domain.Entities.User user,
        OutboxRepositoryBuilder outboxRepository)
    {
        var unitOfWork = UnitOfWorkBuilder.Build();
        var loggedUser = LoggedUserBuilder.Build(user);
        var repository = new UserUpdateOnlyRepositoryBuilder().GetById(user).Build();

        return new RequestDeleteUserUseCase(repository, outboxRepository.Build(), loggedUser, unitOfWork);
    }
}
