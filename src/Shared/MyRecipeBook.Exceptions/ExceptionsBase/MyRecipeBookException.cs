using System.Net;

namespace MyRecipeBook.Exceptions.ExceptionsBase;
public abstract class MyRecipeBookException : SystemException
{
    protected MyRecipeBookException(string message) : base(message) { }

    public virtual bool TokenIsExpired => false;
    public abstract IList<string> GetErrorMessages();
    public abstract HttpStatusCode GetStatusCode();
}
