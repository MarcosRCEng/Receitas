using System.Net;

namespace MyRecipeBook.Exceptions.ExceptionsBase;

public class ForbiddenException : MyRecipeBookException
{
    public ForbiddenException(string message) : base(message)
    {
    }

    public override IList<string> GetErrorMessages() => [Message];

    public override HttpStatusCode GetStatusCode() => HttpStatusCode.Forbidden;
}
