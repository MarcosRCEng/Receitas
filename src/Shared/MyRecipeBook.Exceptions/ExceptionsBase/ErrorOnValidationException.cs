using System.Net;

namespace MyRecipeBook.Exceptions.ExceptionsBase;
[Obsolete("Use ValidationException instead.")]
public class ErrorOnValidationException : ValidationException
{
    public ErrorOnValidationException(IList<string> errorMessages) : base(errorMessages)
    {
    }
}
