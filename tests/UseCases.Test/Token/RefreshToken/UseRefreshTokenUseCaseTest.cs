using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Tokens;
using FluentAssertions;
using MyRecipeBook.Application.UseCases.Token.RefreshToken;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Domain.ValueObjects;
using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;
using Xunit;

namespace UseCases.Test.Token.RefreshToken;
public class UseRefreshTokenUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        (var user, _) = UserBuilder.Build();
        var refreshToken = RefreshTokenBuilder.Build(user);

        var useCase = CreateUseCase(refreshToken);

        var result = await useCase.Execute(new RequestNewTokenJson
        {
            RefreshToken = refreshToken.Value
        });

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Error_RefreshToken_Not_Found()
    {
        var useCase = CreateUseCase();

        var act = async () => await useCase.Execute(new RequestNewTokenJson
        {
            RefreshToken = string.Empty
        });

        (await act.Should().ThrowAsync<RefreshTokenNotFoundException>())
            .Where(e => e.Message.Equals(ResourceMessagesException.EXPIRED_SESSION));
    }

    [Fact]
    public async Task Error_RefreshToken_Expired()
    {
        (var user, _) = UserBuilder.Build();
        var refreshToken = RefreshTokenBuilder.Build(user, expiresOn: DateTime.UtcNow.AddDays(-1));

        var useCase = CreateUseCase(refreshToken);

        var act = async () => await useCase.Execute(new RequestNewTokenJson
        {
            RefreshToken = refreshToken.Value
        });

        (await act.Should().ThrowAsync<RefreshTokenExpiredException>())
            .Where(e => e.Message.Equals(ResourceMessagesException.INVALID_SESSION));
    }

    [Fact]
    public async Task Error_RefreshToken_Revoked()
    {
        (var user, _) = UserBuilder.Build();
        var refreshToken = RefreshTokenBuilder.Build(user, revokedAt: DateTime.UtcNow);

        var useCase = CreateUseCase(refreshToken);

        var act = async () => await useCase.Execute(new RequestNewTokenJson
        {
            RefreshToken = refreshToken.Value
        });

        (await act.Should().ThrowAsync<RefreshTokenNotFoundException>())
            .Where(e => e.Message.Equals(ResourceMessagesException.EXPIRED_SESSION));
    }

    private static UseRefreshTokenUseCase CreateUseCase(MyRecipeBook.Domain.Entities.RefreshToken? refreshToken = null)
    {
        var unitOfWork = UnitOfWorkBuilder.Build();
        var accessTokenGenerator = JwtTokenGeneratorBuilder.Build();
        var refreshTokenGenerator = RefreshTokenGeneratorBuilder.Build();
        var tokenRepository = new TokenRepositoryBuilder().Get(refreshToken).Build();

        return new UseRefreshTokenUseCase(unitOfWork, tokenRepository, refreshTokenGenerator, accessTokenGenerator);
    }
}
