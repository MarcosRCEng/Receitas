using Moq;
using MyRecipeBook.Domain.Repositories.User;

namespace CommonTestUtilities.Repositories;

public class UserWriteOnlyRepositoryBuilder
{
    public static IUserWriteOnlyRepository Build()
    {
        var mock = new Mock<IUserWriteOnlyRepository>();

        mock
            .Setup(repository => repository.Add(It.IsAny<MyRecipeBook.Domain.Entities.User>()))
            .Callback<MyRecipeBook.Domain.Entities.User>(user => user.Id = 1)
            .Returns(Task.CompletedTask);

        return mock.Object;
    }
}
