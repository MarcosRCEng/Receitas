using CommonTestUtilities.Cryptography;
using CommonTestUtilities.Entities;
using CommonTestUtilities.LoggedUser;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.User.ChangePassword;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;
using Xunit;

namespace UseCases.Test.User.ChangePassword;

public class ChangePasswordUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        (var user, var password) = UserBuilder.Build();

        var request = RequestChangePasswordJsonBuilder.Build();
        request.Password = password;

        var useCase = CreateUseCase(user);

        Func<Task> act = async () => await useCase.Execute(request);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Error_NewPassword_Empty()
    {
        (var user, var password) = UserBuilder.Build();

        var request = new RequestChangePasswordJson
        {
            Password = password,
            NewPassword = string.Empty
        };

        var useCase = CreateUseCase(user);

        Func<Task> act = async () => { await useCase.Execute(request); };

        (await act.Should().ThrowAsync<ValidationException>())
            .Where(e => e.GetErrorMessages().Count == 1 &&
                e.GetErrorMessages().Contains(ResourceMessagesException.PASSWORD_EMPTY));
    }

    [Fact]
    public async Task Error_CurrentPassword_Different()
    {
        (var user, var password) = UserBuilder.Build();

        var request = RequestChangePasswordJsonBuilder.Build();

        var useCase = CreateUseCase(user);

        Func<Task> act = async () => { await useCase.Execute(request); };

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.GetErrorMessages().Count == 1 &&
                e.GetErrorMessages().Contains(ResourceMessagesException.PASSWORD_DIFFERENT_CURRENT_PASSWORD));
    }

    [Fact]
    public async Task Error_User_Not_Found()
    {
        (var user, var password) = UserBuilder.Build();

        var request = RequestChangePasswordJsonBuilder.Build();
        request.Password = password;

        var useCase = CreateUseCase(user, repositoryUserMissing: true);

        Func<Task> act = async () => await useCase.Execute(request);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .Where(e => e.GetErrorMessages().Count == 1 &&
                e.GetErrorMessages().Contains(ResourceMessagesException.INVALID_SESSION));
    }

    private static ChangePasswordUseCase CreateUseCase(
        MyRecipeBook.Domain.Entities.User user,
        bool repositoryUserMissing = false)
    {
        var unitOfWork = UnitOfWorkBuilder.Build();
        var userFromRepository = repositoryUserMissing ? null : user;
        var userUpdateRepository = new UserUpdateOnlyRepositoryBuilder().GetById(user.Id, userFromRepository).Build();
        var loggedUser = LoggedUserBuilder.Build(user);
        var passwordEncripter = PasswordEncripterBuilder.Build();

        return new ChangePasswordUseCase(loggedUser, passwordEncripter, userUpdateRepository, unitOfWork);
    }
}
