namespace MyRecipeBook.Domain.Entities;

public class OutboxMessage : EntityBase
{
    private OutboxMessage()
    {
        // EF Core materializes messages through the parameterless constructor.
    }

    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime? ProcessedOn { get; private set; }
    public int RetryCount { get; private set; }
    public string? Error { get; private set; }

    public static OutboxMessage Create(string type, string payload)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Outbox message type cannot be empty.", nameof(type));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Outbox message payload cannot be empty.", nameof(payload));

        return new OutboxMessage
        {
            Type = type.Trim(),
            Payload = payload
        };
    }

    public void MarkAsProcessed(DateTime? processedOn = null)
    {
        ProcessedOn = processedOn ?? DateTime.UtcNow;
        Error = null;
    }

    public void MarkAsFailed(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Outbox error cannot be empty.", nameof(error));

        RetryCount++;
        Error = error;
    }
}
