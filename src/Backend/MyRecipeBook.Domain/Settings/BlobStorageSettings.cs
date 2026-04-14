namespace MyRecipeBook.Domain.Settings;

public class BlobStorageSettings
{
    public const string SectionName = "BlobStorage";
    public const string LegacySectionName = "Settings:BlobStorage";

    public string ConnectionString { get; set; } = string.Empty;
    public string Azure { get; set; } = string.Empty;

    public string GetConnectionString()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString).Equals(false))
            return ConnectionString;

        return Azure;
    }

    public bool IsConfigured() => string.IsNullOrWhiteSpace(GetConnectionString()).Equals(false);
}
