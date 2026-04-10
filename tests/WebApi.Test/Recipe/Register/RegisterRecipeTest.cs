using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Exceptions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Recipe.Register;
public class RegisterRecipeTest : MyRecipeBookClassFixture
{
    private const string METHOD = "recipe";

    private readonly string _email;
    private readonly string _password;

    public RegisterRecipeTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _email = factory.GetEmail();
        _password = factory.GetPassword();
    }

    [Fact]
    public async Task Success()
    {
        var request = RequestRegisterRecipeFormDataBuilder.Build();

        var token = await GetAccessToken();

        var response = await DoPostFormData(method: METHOD, request: request, token: token);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        responseData.RootElement.GetProperty("title").GetString().Should().Be(request.Title);
        var recipeId = responseData.RootElement.GetProperty("id").GetString();
        recipeId.Should().NotBeNullOrWhiteSpace();

        var getRecipeResponse = await DoGet($"{METHOD}/{recipeId}", token);

        getRecipeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var getRecipeResponseBody = await getRecipeResponse.Content.ReadAsStreamAsync();

        var getRecipeResponseData = await JsonDocument.ParseAsync(getRecipeResponseBody);

        getRecipeResponseData.RootElement.GetProperty("title").GetString().Should().Be(request.Title);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Title_Empty(string culture)
    {
        var request = RequestRegisterRecipeFormDataBuilder.Build();
        request.Title = string.Empty;

        var token = await GetAccessToken();

        var response = await DoPostFormData(method: METHOD, request: request, token: token, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("RECIPE_TITLE_EMPTY", new CultureInfo(culture));

        errors.Should().HaveCount(1).And.Contain(c => c.GetString()!.Equals(expectedMessage));
    }

    private async Task<string> GetAccessToken()
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
            .GetProperty("accessToken")
            .GetString()!;
    }
}
