namespace MyRecipeBook.Domain.Entities;

public class OutboxMessage : EntityBase
{
    public required string Type { get; set; } = string.Empty;
    public required string Payload { get; set; } = string.Empty;
    public DateTime? ProcessedOn { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}
