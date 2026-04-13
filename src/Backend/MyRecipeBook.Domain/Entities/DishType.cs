using System.ComponentModel.DataAnnotations.Schema;

namespace MyRecipeBook.Domain.Entities;

[Table("DishTypes")]
public class DishType : EntityBase
{
    private DishType()
    {
        // EF Core needs a parameterless constructor to hydrate child entities.
    }

    public Enums.DishType Type { get; private set; }
    public long RecipeId { get; private set; }

    public static DishType Create(Enums.DishType type)
    {
        return new DishType
        {
            Type = type
        };
    }
}
