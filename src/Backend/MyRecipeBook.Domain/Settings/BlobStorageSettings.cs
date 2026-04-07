namespace MyRecipeBook.Domain.Settings;

public class BlobStorageSettings
{
    public const string SectionName = "Settings:BlobStorage";

    public string Azure { get; set; } = string.Empty;

    public bool IsConfigured() => string.IsNullOrWhiteSpace(Azure).Equals(false);
}
