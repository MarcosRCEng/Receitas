using FluentAssertions;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Exceptions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Token.UseRefreshToken;
public class GetNewAccessTokenTest : MyRecipeBookClassFixture
{
    private const string METHOD = "token";

    private readonly string _email;
    private readonly string _password;

    public GetNewAccessTokenTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _email = factory.GetEmail();
        _password = factory.GetPassword();
    }

    [Fact]
    public async Task Success()
    {
        var request = new RequestNewTokenJson
        {
            RefreshToken = await GetFreshRefreshToken()
        };

        var response = await DoPost($"{METHOD}/refresh-token", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        responseData.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
        responseData.RootElement.GetProperty("refreshToken").GetString().Should().NotBeNullOrWhiteSpace().And.NotBe(request.RefreshToken);
    }

    [Fact]
    public async Task Error_Old_RefreshToken_Cannot_Be_Reused_After_Rotation()
    {
        var request = new RequestNewTokenJson
        {
            RefreshToken = await GetFreshRefreshToken()
        };

        var firstResponse = await DoPost($"{METHOD}/refresh-token", request);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await DoPost($"{METHOD}/refresh-token", request);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<string> GetFreshRefreshToken()
    {
        var loginResponse = await DoPost("login", new RequestLoginJson
        {
            Email = _email,
            Password = _password
        });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await loginResponse.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        return responseData.RootElement
            .GetProperty("tokens")
            .GetProperty("refreshToken")
            .GetString()!;
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Login_Invalid(string culture)
    {
        var request = new RequestNewTokenJson
        {
            RefreshToken = "InvalidRefreshToken"
        };

        var response = await DoPost($"{METHOD}/refresh-token", request, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EXPIRED_SESSION", new CultureInfo(culture));

        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
