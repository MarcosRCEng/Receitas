using FluentValidation;
using FluentValidation.Validators;
using MyRecipeBook.Domain.ValueObjects;
using MyRecipeBook.Exceptions;

namespace MyRecipeBook.Application.SharedValidators;

public class EmailValidator<T> : PropertyValidator<T, string>
{
    public override bool IsValid(ValidationContext<T> context, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            context.MessageFormatter.AppendArgument("ErrorMessage", ResourceMessagesException.EMAIL_EMPTY);

            return false;
        }

        try
        {
            _ = new Email(email);

            return true;
        }
        catch (ArgumentException)
        {
            context.MessageFormatter.AppendArgument("ErrorMessage", ResourceMessagesException.EMAIL_INVALID);

            return false;
        }
    }

    public override string Name => "EmailValidator";

    protected override string GetDefaultMessageTemplate(string errorCode) => "{ErrorMessage}";
}
