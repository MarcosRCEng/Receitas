using System.ComponentModel.DataAnnotations.Schema;

namespace MyRecipeBook.Domain.Entities;

[Table("Ingredients")]
public class Ingredient : EntityBase
{
    private Ingredient()
    {
        // EF Core needs a parameterless constructor to hydrate child entities.
    }

    public string Item { get; private set; } = string.Empty;
    public long RecipeId { get; private set; }

    public static Ingredient Create(string item)
    {
        if (string.IsNullOrWhiteSpace(item))
            throw new ArgumentException("Ingredient item cannot be empty.", nameof(item));

        return new Ingredient
        {
            Item = item.Trim()
        };
    }
}
