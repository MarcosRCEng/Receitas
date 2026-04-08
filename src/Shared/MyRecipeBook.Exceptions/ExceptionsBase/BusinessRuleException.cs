using System.Net;

namespace MyRecipeBook.Exceptions.ExceptionsBase;

public class BusinessRuleException : MyRecipeBookException
{
    public BusinessRuleException(string message) : base(message)
    {
    }

    public BusinessRuleException(IList<string> errorMessages) : base(string.Empty)
    {
        ErrorMessages = errorMessages;
    }

    private IList<string> ErrorMessages { get; } = [];

    public override IList<string> GetErrorMessages() =>
        ErrorMessages.Count > 0 ? ErrorMessages : [Message];

    public override HttpStatusCode GetStatusCode() => HttpStatusCode.Conflict;
}
