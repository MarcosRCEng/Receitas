using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace WebApi.Test.Swagger;

public class GetSwaggerJsonTest : IClassFixture<DevelopmentSwaggerWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public GetSwaggerJsonTest(DevelopmentSwaggerWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Success()
    {
        var response = await _httpClient.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);

        var root = document.RootElement;
        var paths = root.GetProperty("paths");

        paths.TryGetProperty("/dashboard", out _).Should().BeTrue();
        paths.TryGetProperty("/recipe/{id}", out var recipePath).Should().BeTrue();

        var idParameter = recipePath
            .GetProperty("get")
            .GetProperty("parameters")
            .EnumerateArray()
            .Single(parameter => parameter.GetProperty("name").GetString() == "id");

        idParameter.GetProperty("schema").GetProperty("type").GetString().Should().Be("string");
    }
}
