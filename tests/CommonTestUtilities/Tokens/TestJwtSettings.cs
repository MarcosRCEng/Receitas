namespace CommonTestUtilities.Tokens;

public static class TestJwtSettings
{
    // Single source of truth for JWT values used by WebApi.Test.
    // Keep handcrafted test tokens and the API test host aligned to avoid false 401s.
    public const string SigningKey = "test-only-signing-key-not-for-production";
    public const uint ExpirationTimeMinutes = 1000;
    public const string Issuer = "MyRecipeBook";
    public const string Audience = "MyRecipeBook";
}
