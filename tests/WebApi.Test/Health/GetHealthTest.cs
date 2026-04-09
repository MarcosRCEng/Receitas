using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace WebApi.Test.Health;

public class GetHealthTest : MyRecipeBookClassFixture
{
    private const string METHOD = "health";

    public GetHealthTest(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Success()
    {
        var response = await DoGet(METHOD);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        responseData.RootElement.GetProperty("status").GetString().Should().Be("Healthy");

        var checks = responseData.RootElement.GetProperty("checks");

        checks.GetProperty("database").GetProperty("status").GetString().Should().Be("Healthy");
        checks.GetProperty("service_bus").GetProperty("status").GetString().Should().Be("Healthy");
        checks.GetProperty("blob_storage").GetProperty("status").GetString().Should().Be("Healthy");
    }
}
