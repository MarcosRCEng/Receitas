namespace MyRecipeBook.Domain.Entities;
public class RefreshToken : EntityBase
{
    private RefreshToken()
    {
        // EF Core materializes tokens through the parameterless constructor.
    }

    public string Value { get; private set; } = string.Empty;
    public long UserId { get; private set; }
    public DateTime ExpiresOn { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public User User { get; private set; } = default!;

    public static RefreshToken Create(string value, long userId, DateTime expiresOn)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Refresh token value cannot be empty.", nameof(value));

        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId), "Refresh token user id must be greater than zero.");

        return new RefreshToken
        {
            Value = value.Trim(),
            UserId = userId,
            ExpiresOn = expiresOn
        };
    }

    public static RefreshToken Create(string value, User user, DateTime expiresOn)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Refresh token value cannot be empty.", nameof(value));

        var refreshToken = new RefreshToken
        {
            Value = value.Trim(),
            UserId = user.Id,
            ExpiresOn = expiresOn,
            User = user
        };

        return refreshToken;
    }

    public bool IsRevoked => RevokedAt is not null;

    public bool IsExpired(DateTime referenceTimeUtc) => ExpiresOn < referenceTimeUtc;

    public void Revoke(DateTime? revokedAt = null)
    {
        RevokedAt = revokedAt ?? DateTime.UtcNow;
    }
}
