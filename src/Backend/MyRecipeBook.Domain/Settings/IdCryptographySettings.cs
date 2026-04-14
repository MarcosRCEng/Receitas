namespace MyRecipeBook.Domain.Settings;

public class IdCryptographySettings
{
    public const string SectionName = "IdCryptography";
    public const string LegacySectionName = "Settings";

    public string Alphabet { get; set; } = string.Empty;
    public string IdCryptographyAlphabet { get; set; } = string.Empty;

    public string GetAlphabet()
    {
        if (string.IsNullOrWhiteSpace(Alphabet).Equals(false))
            return Alphabet;

        return IdCryptographyAlphabet;
    }
}
