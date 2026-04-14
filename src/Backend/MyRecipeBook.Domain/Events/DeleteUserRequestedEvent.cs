namespace MyRecipeBook.Domain.Events;

public class DeleteUserRequestedEvent
{
    public Guid UserIdentifier { get; set; }
    public Guid? RequestId { get; set; }
    public DateTime? RequestedOnUtc { get; set; }
}
