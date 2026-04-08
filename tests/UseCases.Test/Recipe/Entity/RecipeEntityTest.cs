using FluentAssertions;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Enums;
using MyRecipeBook.Domain.ValueObjects;
using Xunit;

namespace UseCases.Test.Recipe.Entity;

public class RecipeEntityTest
{
    [Fact]
    public void UpdateTitle_Should_Trim_And_Update_Title()
    {
        var recipe = MyRecipeBook.Domain.Entities.Recipe.Create(
            userId: 1,
            title: new RecipeTitle("Recipe"),
            cookingTime: CookingTime.Less_10_Minutes,
            difficulty: Difficulty.Low,
            ingredients: ["Salt"],
            instructions: [new Instruction { Step = 1, Text = "Mix" }],
            dishTypes: [MyRecipeBook.Domain.Enums.DishType.Breakfast]);

        recipe.UpdateTitle(new RecipeTitle("  New title  "));

        recipe.Title.Should().Be("New title");
    }

    [Fact]
    public void AddIngredient_Should_Add_New_Ingredient()
    {
        var recipe = MyRecipeBook.Domain.Entities.Recipe.Create(
            userId: 1,
            title: new RecipeTitle("Recipe"),
            cookingTime: CookingTime.Less_10_Minutes,
            difficulty: Difficulty.Low,
            ingredients: ["Salt"],
            instructions: [new Instruction { Step = 1, Text = "Mix" }],
            dishTypes: [MyRecipeBook.Domain.Enums.DishType.Breakfast]);

        recipe.AddIngredient("Sugar");

        recipe.Ingredients.Should().Contain(ingredient => ingredient.Item == "Sugar");
    }
}
