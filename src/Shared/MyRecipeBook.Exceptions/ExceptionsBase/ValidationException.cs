using System.Net;

namespace MyRecipeBook.Exceptions.ExceptionsBase;

public class ValidationException : MyRecipeBookException
{
    private readonly IList<string> _errorMessages;

    public ValidationException(IList<string> errorMessages) : base(string.Empty)
    {
        _errorMessages = errorMessages;
    }

    public override IList<string> GetErrorMessages() => _errorMessages;

    public override HttpStatusCode GetStatusCode() => HttpStatusCode.BadRequest;
}
