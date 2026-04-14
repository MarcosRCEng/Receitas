namespace MyRecipeBook.Domain.Settings;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public const string LegacySectionName = "Settings:Jwt";

    public string SigningKey { get; set; } = string.Empty;
    public uint ExpirationTimeMinutes { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
