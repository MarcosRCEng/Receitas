namespace MyRecipeBook.Domain.Settings;

public class OpenAISettings
{
    public const string SectionName = "OpenAI";
    public const string LegacySectionName = "Settings:OpenAI";

    public string ApiKey { get; set; } = string.Empty;
}
