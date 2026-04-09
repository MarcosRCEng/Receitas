using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using MyRecipeBook.API.Middleware;
using Xunit;

namespace WebApi.Test.Middleware;

public class CorrelationIdMiddlewareTest
{
    [Fact]
    public async Task Should_Create_CorrelationId_When_Header_Is_Missing()
    {
        var context = CreateHttpContext();
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        context.TraceIdentifier.Should().NotBeNullOrWhiteSpace();
        context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().Should().Be(context.TraceIdentifier);
        context.Items[CorrelationIdMiddleware.ItemKey].Should().Be(context.TraceIdentifier);
    }

    [Fact]
    public async Task Should_Reuse_CorrelationId_From_Request_Header()
    {
        var context = CreateHttpContext();
        const string correlationId = "request-correlation-id";
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        context.TraceIdentifier.Should().Be(correlationId);
        context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().Should().Be(correlationId);
    }

    private static CorrelationIdMiddleware CreateMiddleware() =>
        new(_ => Task.CompletedTask, NullLogger<CorrelationIdMiddleware>.Instance);

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        return context;
    }
}
