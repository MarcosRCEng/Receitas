namespace MyRecipeBook.Domain.ValueObjects;

public readonly record struct RecipeTitle
{
    public string Value { get; }

    public RecipeTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Recipe title cannot be empty.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;

    public static implicit operator string(RecipeTitle title) => title.Value;
}
