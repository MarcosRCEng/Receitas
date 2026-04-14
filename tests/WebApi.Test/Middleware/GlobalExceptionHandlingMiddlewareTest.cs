using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using MyRecipeBook.API.Middleware;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;
using System.Net;
using System.Text.Json;
using Xunit;

namespace WebApi.Test.Middleware;

public class GlobalExceptionHandlingMiddlewareTest
{
    [Fact]
    public async Task Should_Map_ValidationException_To_BadRequest()
    {
        var middleware = CreateMiddleware(_ => throw new ValidationException(["invalid request"]));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        (await GetErrors(context)).Should().ContainSingle().And.Contain("invalid request");
    }

    [Fact]
    public async Task Should_Map_BusinessRuleException_To_Conflict()
    {
        var middleware = CreateMiddleware(_ => throw new BusinessRuleException("rule broken"));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        (await GetErrors(context)).Should().ContainSingle().And.Contain("rule broken");
    }

    [Fact]
    public async Task Should_Map_Unhandled_Exception_To_InternalServerError()
    {
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("unexpected"));
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        (await GetErrors(context)).Should().ContainSingle().And.Contain(ResourceMessagesException.UNKNOWN_ERROR);
    }

    [Fact]
    public async Task Should_Preserve_Token_Expired_Metadata()
    {
        var middleware = CreateMiddleware(_ => throw new RefreshTokenExpiredException());
        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        (await GetErrors(context)).Should().ContainSingle().And.Contain(ResourceMessagesException.INVALID_SESSION);
        (await GetTokenIsExpired(context)).Should().BeTrue();
    }

    private static GlobalExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next) =>
        new(next, NullLogger<GlobalExceptionHandlingMiddleware>.Instance);

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        return context;
    }

    private static async Task<IList<string>> GetErrors(HttpContext context)
    {
        context.Response.Body.Position = 0;

        using var responseDocument = await JsonDocument.ParseAsync(context.Response.Body);

        return responseDocument.RootElement
            .GetProperty("errors")
            .EnumerateArray()
            .Select(error => error.GetString()!)
            .ToList();
    }

    private static async Task<bool> GetTokenIsExpired(HttpContext context)
    {
        context.Response.Body.Position = 0;

        using var responseDocument = await JsonDocument.ParseAsync(context.Response.Body);

        return responseDocument.RootElement.GetProperty("tokenIsExpired").GetBoolean();
    }
}
