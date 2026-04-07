namespace MyRecipeBook.Domain.Settings;

public class IdCryptographySettings
{
    public const string SectionName = "Settings";

    public string IdCryptographyAlphabet { get; set; } = string.Empty;
}
