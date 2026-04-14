using CommonTestUtilities.Entities;
using CommonTestUtilities.LoggedUser;
using CommonTestUtilities.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyRecipeBook.Application.UseCases.User.Delete.Request;
using MyRecipeBook.Domain.Repositories;
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
        payload.RequestId.Should().NotBeNull();
        payload.RequestedOnUtc.Should().NotBeNull();
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

    [Fact]
    public async Task Should_Not_Create_New_Outbox_Message_When_User_Is_Already_Inactive()
    {
        (var user, _) = UserBuilder.Build();
        user.Deactivate();

        var outboxRepository = new OutboxRepositoryBuilder();
        var unitOfWork = new Mock<IUnitOfWork>();
        var loggedUser = LoggedUserBuilder.Build(user);
        var repository = new UserUpdateOnlyRepositoryBuilder().GetById(user.Id, user).Build();
        var useCase = new RequestDeleteUserUseCase(
            repository,
            outboxRepository.Build(),
            loggedUser,
            unitOfWork.Object,
            new Mock<ILogger<RequestDeleteUserUseCase>>().Object);

        var act = async () => await useCase.Execute();

        await act.Should().NotThrowAsync();

        outboxRepository.Messages.Should().BeEmpty();
        unitOfWork.Verify(workflow => workflow.Commit(), Times.Never);
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

        return new RequestDeleteUserUseCase(
            repository,
            outboxRepository.Build(),
            loggedUser,
            unitOfWork,
            new Mock<ILogger<RequestDeleteUserUseCase>>().Object);
    }
}
