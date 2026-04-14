using CommonTestUtilities.Tokens;
using FluentAssertions;
using MyRecipeBook.Exceptions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using Xunit;

namespace WebApi.Test.Authorization;

public class AuthorizationErrorContractTest : MyRecipeBookClassFixture
{
    private readonly Guid _userIdentifier;

    public AuthorizationErrorContractTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _userIdentifier = factory.GetUserIdentifier();
    }

    [Fact]
    public async Task Error_Without_Token_Should_Return_Standard_Contract()
    {
        var response = await DoGet("dashboard", culture: "pt-BR");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var responseData = await ReadResponse(response);

        responseData.RootElement.GetProperty("tokenIsExpired").GetBoolean().Should().BeFalse();
        responseData.RootElement.GetProperty("errors").EnumerateArray()
            .Select(error => error.GetString())
            .Should()
            .ContainSingle()
            .Which.Should().Be(ResourceMessagesException.ResourceManager.GetString("NO_TOKEN", new CultureInfo("pt-BR")));
    }

    [Fact]
    public async Task Error_Invalid_Token_Should_Return_Standard_Contract()
    {
        var response = await DoGet("dashboard", token: "tokenInvalid", culture: "pt-BR");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var responseData = await ReadResponse(response);

        responseData.RootElement.GetProperty("tokenIsExpired").GetBoolean().Should().BeFalse();
        responseData.RootElement.GetProperty("errors").EnumerateArray()
            .Select(error => error.GetString())
            .Should()
            .ContainSingle()
            .Which.Should().Be(ResourceMessagesException.ResourceManager.GetString("INVALID_SESSION", new CultureInfo("pt-BR")));
    }

    [Fact]
    public async Task Error_User_Without_Permission_Should_Return_Forbidden_Contract()
    {
        var response = await DoGet("dashboard", JwtTokenGeneratorBuilder.Build().Generate(Guid.NewGuid()), culture: "pt-BR");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var responseData = await ReadResponse(response);

        responseData.RootElement.GetProperty("tokenIsExpired").GetBoolean().Should().BeFalse();
        responseData.RootElement.GetProperty("errors").EnumerateArray()
            .Select(error => error.GetString())
            .Should()
            .ContainSingle()
            .Which.Should().Be(ResourceMessagesException.ResourceManager.GetString("USER_WITHOUT_PERMISSION_ACCESS_RESOURCE", new CultureInfo("pt-BR")));
    }

    [Fact]
    public async Task Success_Authenticated_User_Should_Not_Be_Affected()
    {
        var response = await DoGet("dashboard", JwtTokenGeneratorBuilder.Build().Generate(_userIdentifier));

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    private static async Task<JsonDocument> ReadResponse(HttpResponseMessage response)
    {
        await using var responseBody = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(responseBody);
    }
}
