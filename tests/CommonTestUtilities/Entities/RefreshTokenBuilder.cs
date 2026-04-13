using Bogus;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.ValueObjects;

namespace CommonTestUtilities.Entities;
public class RefreshTokenBuilder
{
    public static RefreshToken Build(User user, DateTime? expiresOn = null, DateTime? revokedAt = null)
    {
        var faker = new Faker();
        var refreshToken = RefreshToken.Create(
            faker.Lorem.Word(),
            user,
            expiresOn ?? DateTime.UtcNow.AddDays(MyRecipeBookRuleConstants.REFRESH_TOKEN_EXPIRATION_DAYS));

        refreshToken.Id = 1;

        if (revokedAt.HasValue)
            refreshToken.Revoke(revokedAt.Value);

        return refreshToken;
    }
}
