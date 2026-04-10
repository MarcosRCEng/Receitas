using CommonTestUtilities.Tokens;
using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace WebApi.Test.Versioning;

public class ApiVersioningTest : MyRecipeBookClassFixture
{
    private readonly string _name;
    private readonly string _email;
    private readonly Guid _userIdentifier;

    public ApiVersioningTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _name = factory.GetName();
        _email = factory.GetEmail();
        _userIdentifier = factory.GetUserIdentifier();
    }

    [Fact]
    public async Task Supports_Url_Versioning()
    {
        var token = JwtTokenGeneratorBuilder.Build().Generate(_userIdentifier);

        var response = await DoGet("api/v1/user", token: token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        responseData.RootElement.GetProperty("name").GetString().Should().Be(_name);
        responseData.RootElement.GetProperty("email").GetString().Should().Be(_email);
    }

    [Fact]
    public async Task Supports_Header_Versioning_On_Legacy_Path()
    {
        var token = JwtTokenGeneratorBuilder.Build().Generate(_userIdentifier);

        var response = await DoGet("user", token: token, apiVersion: "1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        responseData.RootElement.GetProperty("name").GetString().Should().Be(_name);
        responseData.RootElement.GetProperty("email").GetString().Should().Be(_email);
    }

    [Fact]
    public async Task Supports_Multiple_Versions_Simultaneously()
    {
        var token = JwtTokenGeneratorBuilder.Build().Generate(_userIdentifier);

        var v1Response = await DoGet("api/v1/user", token: token);
        var v2Response = await DoGet("api/v2/user", token: token);

        v1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        v2Response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
