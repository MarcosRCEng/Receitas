using System.ComponentModel.DataAnnotations.Schema;

namespace MyRecipeBook.Domain.Entities;

[Table("Instructions")]
public class Instruction : EntityBase
{
    private Instruction()
    {
        // EF Core needs a parameterless constructor to hydrate child entities.
    }

    public int Step { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public long RecipeId { get; private set; }

    public static Instruction Create(int step, string text)
    {
        if (step <= 0)
            throw new ArgumentOutOfRangeException(nameof(step), "Instruction step must be greater than zero.");

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Instruction text cannot be empty.", nameof(text));

        return new Instruction
        {
            Step = step,
            Text = text.Trim()
        };
    }
}
