namespace MyRecipeBook.Domain.Settings;

public class AzureServiceBusSettings
{
    public const string SectionName = "ServiceBus";
    public const string LegacySectionName = "Settings:ServiceBus";

    public string ConnectionString { get; set; } = string.Empty;
    public string DeleteUserAccount { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;

    public string GetConnectionString()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString).Equals(false))
            return ConnectionString;

        return DeleteUserAccount;
    }

    public bool IsConfigured()
    {
        return string.IsNullOrWhiteSpace(GetConnectionString()).Equals(false)
            && string.IsNullOrWhiteSpace(QueueName).Equals(false);
    }
}
