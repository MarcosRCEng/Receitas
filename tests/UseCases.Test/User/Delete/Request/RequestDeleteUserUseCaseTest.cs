using CommonTestUtilities.Entities;
using CommonTestUtilities.LoggedUser;
using CommonTestUtilities.Repositories;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.User.Delete.Request;
using MyRecipeBook.Domain.Events;
using MyRecipeBook.Domain.ValueObjects;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;
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

    [Fact]
    public async Task Error_User_Not_Found()
    {
        (var user, _) = UserBuilder.Build();

        var outboxRepository = new OutboxRepositoryBuilder();

        var useCase = CreateUseCase(user, outboxRepository, repositoryUserMissing: true);

        var act = async () => await useCase.Execute();

        await act.Should().ThrowAsync<UnauthorizedException>()
            .Where(e => e.GetErrorMessages().Count == 1 &&
                e.GetErrorMessages().Contains(ResourceMessagesException.INVALID_SESSION));
    }

    private static RequestDeleteUserUseCase CreateUseCase(
        MyRecipeBook.Domain.Entities.User user,
        OutboxRepositoryBuilder outboxRepository,
        bool repositoryUserMissing = false)
    {
        var unitOfWork = UnitOfWorkBuilder.Build();
        var loggedUser = LoggedUserBuilder.Build(user);
        var userFromRepository = repositoryUserMissing ? null : user;
        var repository = new UserUpdateOnlyRepositoryBuilder().GetById(user.Id, userFromRepository).Build();

        return new RequestDeleteUserUseCase(repository, outboxRepository.Build(), loggedUser, unitOfWork);
    }
}
