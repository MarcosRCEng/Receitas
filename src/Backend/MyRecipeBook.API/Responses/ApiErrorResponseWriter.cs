using System.Net;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Exceptions.ExceptionsBase;

namespace MyRecipeBook.API.Responses;

public static class ApiErrorResponseWriter
{
    public static Task WriteAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        IList<string> errors,
        bool tokenIsExpired = false,
        CancellationToken cancellationToken = default)
    {
        if (context.Response.HasStarted)
            throw new InvalidOperationException("The response has already started.");

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(
            new ResponseErrorJson(errors, tokenIsExpired),
            cancellationToken);
    }

    public static Task WriteAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string error,
        bool tokenIsExpired = false,
        CancellationToken cancellationToken = default) =>
        WriteAsync(context, statusCode, new List<string> { error }, tokenIsExpired, cancellationToken);

    public static Task WriteAsync(
        HttpContext context,
        MyRecipeBookException exception,
        CancellationToken cancellationToken = default) =>
        WriteAsync(
            context,
            exception.GetStatusCode(),
            exception.GetErrorMessages(),
            exception.TokenIsExpired,
            cancellationToken);
}
