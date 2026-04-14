using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Infrastructure.Security.Tokens.Access.Generator;

namespace CommonTestUtilities.Tokens;

public class JwtTokenGeneratorBuilder
{
    // Always read from TestJwtSettings instead of duplicating literals here.
    public static IAccessTokenGenerator Build() => new JwtTokenGenerator(
        expirationTimeMinutes: TestJwtSettings.ExpirationTimeMinutes,
        signingKey: TestJwtSettings.SigningKey,
        issuer: TestJwtSettings.Issuer,
        audience: TestJwtSettings.Audience);
}
