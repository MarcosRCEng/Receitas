using Bogus;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
using MyRecipeBook.Domain.ValueObjects;

namespace CommonTestUtilities.Entities;
public class RecipeBuilder
{
    public static IList<Recipe> Collection(User user, uint count = 2)
    {
        var list = new List<Recipe>();

        if (count == 0)
            count = 1;

        var recipeId = 1;

        for (int i = 0; i < count; i++)
        {
            var fakeRecipe = Build(user);
            fakeRecipe.Id = recipeId++;

            list.Add(fakeRecipe);
        }

        return list;
    }

    public static Recipe Build(User user)
    {
        var faker = new Faker();

        var recipe = Recipe.Create(
            user.Id,
            new RecipeTitle(faker.Lorem.Word()),
            faker.PickRandom<CookingTime>(),
            faker.PickRandom<Difficulty>(),
            [faker.Commerce.ProductName()],
            [new Instruction
            {
                Id = 1,
                Step = 1,
                Text = faker.Lorem.Paragraph()
            }],
            [faker.PickRandom<MyRecipeBook.Domain.Enums.DishType>()]);

        recipe.Id = 1;
        recipe.SetImageIdentifier($"{Guid.NewGuid()}.png");

        return recipe;
    }
}
