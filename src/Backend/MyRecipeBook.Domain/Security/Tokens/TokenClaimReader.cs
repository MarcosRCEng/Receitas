using System.Security.Claims;

namespace MyRecipeBook.Domain.Security.Tokens;

public static class TokenClaimReader
{
    private const string SubjectClaimType = "sub";

    public static bool TryGetUserIdentifier(ClaimsPrincipal principal, out Guid userIdentifier)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return TryGetUserIdentifier(principal.Claims, out userIdentifier);
    }

    public static bool TryGetUserIdentifier(IEnumerable<Claim> claims, out Guid userIdentifier)
    {
        ArgumentNullException.ThrowIfNull(claims);

        userIdentifier = Guid.Empty;

        var userIdentifierValue = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value
            ?? claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Sid)?.Value
            ?? claims.FirstOrDefault(claim => claim.Type == SubjectClaimType)?.Value;

        return Guid.TryParse(userIdentifierValue, out userIdentifier);
    }
}
