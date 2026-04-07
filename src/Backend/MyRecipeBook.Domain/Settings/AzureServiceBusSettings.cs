namespace MyRecipeBook.Domain.Settings;

public class AzureServiceBusSettings
{
    public const string SectionName = "Settings:ServiceBus";

    public string DeleteUserAccount { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;

    public bool IsConfigured()
    {
        return string.IsNullOrWhiteSpace(DeleteUserAccount).Equals(false)
            && string.IsNullOrWhiteSpace(QueueName).Equals(false);
    }
}
