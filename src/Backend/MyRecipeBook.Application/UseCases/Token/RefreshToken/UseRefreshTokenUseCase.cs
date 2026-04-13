using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.Token;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Domain.ValueObjects;
using MyRecipeBook.Exceptions.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.Token.RefreshToken;
public class UseRefreshTokenUseCase : IUseRefreshTokenUseCase
{
    private readonly ITokenRepository _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public UseRefreshTokenUseCase(
        IUnitOfWork unitOfWork,
        ITokenRepository tokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator,
        IAccessTokenGenerator accessTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _tokenRepository = tokenRepository;
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<ResponseTokensJson> Execute(RequestNewTokenJson request)
    {
        var refreshToken = await _tokenRepository.Get(request.RefreshToken);

        if(refreshToken is null)
            throw new RefreshTokenNotFoundException();

        if (refreshToken.IsRevoked)
            throw new RefreshTokenNotFoundException();

        if (refreshToken.IsExpired(DateTime.UtcNow))
            throw new RefreshTokenExpiredException();

        var newRefreshToken = Domain.Entities.RefreshToken.Create(
            _refreshTokenGenerator.Generate(),
            refreshToken.UserId,
            DateTime.UtcNow.AddDays(MyRecipeBookRuleConstants.REFRESH_TOKEN_EXPIRATION_DAYS));

        await _tokenRepository.SaveNewRefreshToken(newRefreshToken);

        await _unitOfWork.Commit();

        return new ResponseTokensJson
        {
            AccessToken = _accessTokenGenerator.Generate(refreshToken.User.UserIdentifier),
            RefreshToken = newRefreshToken.Value
        };
    }
}
