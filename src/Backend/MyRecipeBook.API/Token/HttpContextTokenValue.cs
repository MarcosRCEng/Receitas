using MyRecipeBook.Domain.Security.Tokens;

using System.Security.Claims;

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
        var userIdentifier = _contextAccessor
            .HttpContext!
            .User
            .Claims
            .First(c => c.Type == ClaimTypes.Sid)
            .Value;

        return Guid.Parse(userIdentifier);
    }
}
