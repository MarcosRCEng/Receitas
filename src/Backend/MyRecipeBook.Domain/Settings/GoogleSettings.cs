namespace MyRecipeBook.Domain.Settings;

public class GoogleSettings
{
    public const string SectionName = "Google";
    public const string LegacySectionName = "Settings:Google";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
