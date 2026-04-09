using Moq;
using MyRecipeBook.Domain.Repositories.Recipe;

namespace CommonTestUtilities.Repositories;
public class RecipeWriteOnlyRepositoryBuilder
{
    public static IRecipeWriteOnlyRepository Build(bool deleteResult = true)
    {
        var mock = new Mock<IRecipeWriteOnlyRepository>();
        mock.Setup(repository => repository.Delete(It.IsAny<long>())).ReturnsAsync(deleteResult);

        return mock.Object;
    }
}
