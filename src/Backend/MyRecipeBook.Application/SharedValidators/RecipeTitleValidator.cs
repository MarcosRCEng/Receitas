using FluentValidation;
using FluentValidation.Validators;
using MyRecipeBook.Domain.ValueObjects;
using MyRecipeBook.Exceptions;

namespace MyRecipeBook.Application.SharedValidators;

public class RecipeTitleValidator<T> : PropertyValidator<T, string>
{
    public override bool IsValid(ValidationContext<T> context, string title)
    {
        try
        {
            _ = new RecipeTitle(title);

            return true;
        }
        catch (ArgumentException)
        {
            context.MessageFormatter.AppendArgument("ErrorMessage", ResourceMessagesException.RECIPE_TITLE_EMPTY);

            return false;
        }
    }

    public override string Name => "RecipeTitleValidator";

    protected override string GetDefaultMessageTemplate(string errorCode) => "{ErrorMessage}";
}
