using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;

namespace MyRecipeBook.API.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if(context.Exception is MyRecipeBookException myRecipeBookException)
            HandleProjectException(myRecipeBookException, context);
        else
            ThrowUnknowException(context);
    }

    private static void HandleProjectException(MyRecipeBookException myRecipeBookException, ExceptionContext context)
    {
        context.Result = new ObjectResult(new ResponseErrorJson(
            myRecipeBookException.GetErrorMessages(),
            myRecipeBookException.TokenIsExpired))
        {
            StatusCode = (int)myRecipeBookException.GetStatusCode()
        };
    }

    private static void ThrowUnknowException(ExceptionContext context)
    {
        context.Result = new ObjectResult(new ResponseErrorJson(ResourceMessagesException.UNKNOWN_ERROR))
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }
}
