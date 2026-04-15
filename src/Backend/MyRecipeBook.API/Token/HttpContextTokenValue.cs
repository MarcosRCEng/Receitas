using MyRecipeBook.Domain.Security.Tokens;

using MyRecipeBook.Exceptions;
using MyRecipeBook.Exceptions.ExceptionsBase;

namespace MyRecipeBook.API.Token;

public class HttpContextTokenValue : ITokenProvider
{
    private readonly IHttpContextAccessor _contextAccessor;

    public HttpContextTokenValue(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public Guid UserIdentifier()
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated is not true)
            throw new UnauthorizedException(ResourceMessagesException.NO_TOKEN);

        if (TokenClaimReader.TryGetUserIdentifier(httpContext.User, out var userIdentifier).Equals(false))
            throw new UnauthorizedException(ResourceMessagesException.INVALID_SESSION);

        return userIdentifier;
    }
}
