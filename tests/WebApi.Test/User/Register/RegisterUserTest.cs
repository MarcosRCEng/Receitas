using CommonTestUtilities.Requests;
using FluentAssertions;
using MyRecipeBook.Exceptions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.User.Register;

public class RegisterUserTest : MyRecipeBookClassFixture
{
    private readonly string METHOD = "user";
    private readonly string _email;

    public RegisterUserTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _email = factory.GetEmail();
    }

    [Fact]
    public async Task Success()
    {
        var request = RequestRegisterUserJsonBuilder.Build();

        var response = await DoPost(method: METHOD, request: request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        responseData.RootElement.GetProperty("name").GetString().Should().NotBeNullOrWhiteSpace().And.Be(request.Name);
        responseData.RootElement.GetProperty("tokens").GetProperty("refreshToken").GetString().Should().NotBeNullOrWhiteSpace();

        var accessToken = responseData.RootElement.GetProperty("tokens").GetProperty("accessToken").GetString();
        accessToken.Should().NotBeNullOrWhiteSpace();

        var profileResponse = await DoGet(METHOD, token: accessToken!);

        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var profileResponseBody = await profileResponse.Content.ReadAsStreamAsync();

        var profileData = await JsonDocument.ParseAsync(profileResponseBody);

        profileData.RootElement.GetProperty("name").GetString().Should().Be(request.Name);
        profileData.RootElement.GetProperty("email").GetString().Should().Be(request.Email);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Empty_Name(string culture)
    {
        var request = RequestRegisterUserJsonBuilder.Build();
        request.Name = string.Empty;

        var response = await DoPost(method: METHOD, request: request, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("NAME_EMPTY", new CultureInfo(culture));

        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Email_Already_Registered(string culture)
    {
        var request = RequestRegisterUserJsonBuilder.Build();
        request.Email = _email;

        var response = await DoPost(method: METHOD, request: request, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EMAIL_ALREADY_REGISTERED", new CultureInfo(culture));

        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Fact]
    public async Task Error_Invalid_Password()
    {
        var request = RequestRegisterUserJsonBuilder.Build(passwordLength: 4);

        var response = await DoPost(method: METHOD, request: request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("INVALID_PASSWORD", new CultureInfo("en"));

        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
