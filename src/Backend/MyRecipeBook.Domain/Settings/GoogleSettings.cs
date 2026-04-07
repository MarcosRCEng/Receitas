namespace MyRecipeBook.Domain.Settings;

public class GoogleSettings
{
    public const string SectionName = "Settings:Google";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
