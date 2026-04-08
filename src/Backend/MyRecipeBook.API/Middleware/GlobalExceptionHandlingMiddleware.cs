using Microsoft.AspNetCore.Mvc;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;

namespace MyRecipeBook.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (MyRecipeBookException exception)
        {
            await WriteErrorResponse(context, exception.GetStatusCode(), exception.GetErrorMessages());
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing request.");

            await WriteErrorResponse(
                context,
                System.Net.HttpStatusCode.InternalServerError,
                [ResourceMessagesException.UNKNOWN_ERROR]);
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context,
        System.Net.HttpStatusCode statusCode,
        IList<string> errors)
    {
        if (context.Response.HasStarted)
            throw new InvalidOperationException("The response has already started.");

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsJsonAsync(new ResponseErrorJson(errors));
    }
}
