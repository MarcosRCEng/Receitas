using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.Events;
using MyRecipeBook.Domain.ValueObjects;
using MyRecipeBook.Infrastructure.DataAccess;
using System.Net;
using System.Text.Json;
using Xunit;

namespace WebApi.Test.User.Delete;

public class DeleteUserTest : MyRecipeBookClassFixture
{
    private const string METHOD = "user";
    private const string LOGIN_METHOD = "login";

    private readonly CustomWebApplicationFactory _factory;
    private readonly string _email;
    private readonly string _password;
    private readonly Guid _userIdentifier;

    public DeleteUserTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _factory = factory;
        _email = factory.GetEmail();
        _password = factory.GetPassword();
        _userIdentifier = factory.GetUserIdentifier();
    }

    [Fact]
    public async Task Success()
    {
        var tokens = await GetTokens();

        var response = await DoDelete(METHOD, tokens.AccessToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MyRecipeBookDbContext>();

        var user = await dbContext.Users.SingleAsync(user => user.UserIdentifier == _userIdentifier);
        user.Active.Should().BeFalse();

        var message = await dbContext.OutboxMessages.SingleAsync();
        message.Type.Should().Be(OutboxMessageTypes.DELETE_USER_REQUESTED);

        var payload = JsonSerializer.Deserialize<DeleteUserRequestedEvent>(message.Payload);
        payload!.UserIdentifier.Should().Be(_userIdentifier);
        payload.RequestId.Should().NotBeNull();
        payload.RequestedOnUtc.Should().NotBeNull();

        var loginResponse = await DoPost(LOGIN_METHOD, new RequestLoginJson
        {
            Email = _email,
            Password = _password
        });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Error_RefreshToken_Cannot_Be_Used_After_User_Deactivation()
    {
        var tokens = await GetTokens();

        var deleteResponse = await DoDelete(METHOD, tokens.AccessToken);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshResponse = await DoPost("token/refresh-token", new RequestNewTokenJson
        {
            RefreshToken = tokens.RefreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<(string AccessToken, string RefreshToken)> GetTokens()
    {
        var loginResponse = await DoPost(LOGIN_METHOD, new RequestLoginJson
        {
            Email = _email,
            Password = _password
        });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await loginResponse.Content.ReadAsStreamAsync();

        var responseData = await JsonDocument.ParseAsync(responseBody);

        return (
            responseData.RootElement
            .GetProperty("tokens")
            .GetProperty("accessToken")
            .GetString()!,
            responseData.RootElement
                .GetProperty("tokens")
                .GetProperty("refreshToken")
                .GetString()!);
    }
}
