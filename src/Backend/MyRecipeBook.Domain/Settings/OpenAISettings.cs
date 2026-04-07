namespace MyRecipeBook.Domain.Settings;

public class OpenAISettings
{
    public const string SectionName = "Settings:OpenAI";

    public string ApiKey { get; set; } = string.Empty;
}
