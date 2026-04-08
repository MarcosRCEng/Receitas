using MyRecipeBook.Domain.Enums;

namespace MyRecipeBook.Domain.Entities;

public class Recipe : EntityBase
{
    private string _title = string.Empty;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public MyRecipeBook.Domain.ValueObjects.RecipeTitle Title => new(_title);
    public CookingTime? CookingTime { get; private set; }
    public Difficulty? Difficulty { get; private set; }
    public IList<Ingredient> Ingredients { get; private set; } = [];
    public IList<Instruction> Instructions { get; private set; } = [];
    public IList<DishType> DishTypes { get; private set; } = [];
    public string? ImageIdentifier { get; private set; }
    public long UserId { get; private set; }

    public static Recipe Create(
        long userId,
        MyRecipeBook.Domain.ValueObjects.RecipeTitle title,
        CookingTime? cookingTime,
        Difficulty? difficulty,
        IEnumerable<string> ingredients,
        IEnumerable<Instruction> instructions,
        IEnumerable<Enums.DishType> dishTypes)
    {
        var recipe = new Recipe();
        recipe.AssignUser(userId);
        recipe.UpdateDetails(title, cookingTime, difficulty);
        recipe.ReplaceIngredients(ingredients);
        recipe.ReplaceInstructions(instructions);
        recipe.ReplaceDishTypes(dishTypes);

        return recipe;
    }

    public void AssignUser(long userId)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId), "Recipe user id must be greater than zero.");

        UserId = userId;
    }

    public void UpdateDetails(MyRecipeBook.Domain.ValueObjects.RecipeTitle title, CookingTime? cookingTime, Difficulty? difficulty)
    {
        UpdateTitle(title);
        CookingTime = cookingTime;
        Difficulty = difficulty;
    }

    public void UpdateTitle(MyRecipeBook.Domain.ValueObjects.RecipeTitle title)
    {
        _title = title.Value;
    }

    public void AddIngredient(string item)
    {
        if (string.IsNullOrWhiteSpace(item))
            throw new ArgumentException("Ingredient item cannot be empty.", nameof(item));

        Ingredients.Add(new Ingredient
        {
            Item = item.Trim()
        });
    }

    public void ReplaceIngredients(IEnumerable<string> ingredients)
    {
        ArgumentNullException.ThrowIfNull(ingredients);

        Ingredients.Clear();

        foreach (var ingredient in ingredients)
            AddIngredient(ingredient);
    }

    public void ReplaceInstructions(IEnumerable<Instruction> instructions)
    {
        ArgumentNullException.ThrowIfNull(instructions);

        Instructions.Clear();

        foreach (var instruction in instructions)
        {
            if (instruction is null)
                throw new ArgumentException("Instruction cannot be null.", nameof(instructions));

            if (instruction.Step <= 0)
                throw new ArgumentException("Instruction step must be greater than zero.", nameof(instructions));

            if (string.IsNullOrWhiteSpace(instruction.Text))
                throw new ArgumentException("Instruction text cannot be empty.", nameof(instructions));

            Instructions.Add(instruction);
        }
    }

    public void ReplaceDishTypes(IEnumerable<Enums.DishType> dishTypes)
    {
        ArgumentNullException.ThrowIfNull(dishTypes);

        DishTypes.Clear();

        foreach (var dishType in dishTypes.Distinct())
        {
            DishTypes.Add(new DishType
            {
                Type = dishType
            });
        }
    }

    public void SetImageIdentifier(string imageIdentifier)
    {
        if (string.IsNullOrWhiteSpace(imageIdentifier))
            throw new ArgumentException("Image identifier cannot be empty.", nameof(imageIdentifier));

        ImageIdentifier = imageIdentifier.Trim();
    }
}
