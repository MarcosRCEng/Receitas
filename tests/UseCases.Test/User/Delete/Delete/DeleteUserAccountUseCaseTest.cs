using CommonTestUtilities.Entities;
using FluentAssertions;
using Moq;
using MyRecipeBook.Application.UseCases.User.Delete.Delete;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Services.Storage;
using Xunit;

namespace UseCases.Test.User.Delete.Delete;

public class DeleteUserAccountUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        (var user, _) = UserBuilder.Build();

        var repositoryRead = new Mock<IUserReadOnlyRepository>();
        var repositoryDelete = new Mock<IUserDeleteOnlyRepository>();
        var blobStorageService = new Mock<IBlobStorageService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repositoryRead
            .Setup(repository => repository.ExistActiveUserWithIdentifier(user.UserIdentifier))
            .ReturnsAsync(true);

        var useCase = CreateUseCase(
            repositoryRead.Object,
            repositoryDelete.Object,
            blobStorageService.Object,
            unitOfWork.Object);

        var act = async () => await useCase.Execute(user.UserIdentifier);

        await act.Should().NotThrowAsync();

        blobStorageService.Verify(service => service.DeleteContainer(user.UserIdentifier), Times.Once);
        repositoryDelete.Verify(repository => repository.DeleteAccount(user.UserIdentifier), Times.Once);
        unitOfWork.Verify(workflow => workflow.Commit(), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Process_When_User_Was_Already_Deleted()
    {
        var userIdentifier = Guid.NewGuid();
        var repositoryRead = new Mock<IUserReadOnlyRepository>();
        var repositoryDelete = new Mock<IUserDeleteOnlyRepository>();
        var blobStorageService = new Mock<IBlobStorageService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repositoryRead
            .Setup(repository => repository.ExistActiveUserWithIdentifier(userIdentifier))
            .ReturnsAsync(false);

        var useCase = CreateUseCase(
            repositoryRead.Object,
            repositoryDelete.Object,
            blobStorageService.Object,
            unitOfWork.Object);

        var act = async () => await useCase.Execute(userIdentifier);

        await act.Should().NotThrowAsync();

        blobStorageService.Verify(service => service.DeleteContainer(It.IsAny<Guid>()), Times.Never);
        repositoryDelete.Verify(repository => repository.DeleteAccount(It.IsAny<Guid>()), Times.Never);
        unitOfWork.Verify(workflow => workflow.Commit(), Times.Never);
    }

    private static DeleteUserAccountUseCase CreateUseCase(
        IUserReadOnlyRepository repositoryRead,
        IUserDeleteOnlyRepository repositoryDelete,
        IBlobStorageService blobStorageService,
        IUnitOfWork unitOfWork)
    {
        return new DeleteUserAccountUseCase(repositoryRead, repositoryDelete, blobStorageService, unitOfWork);
    }
}
