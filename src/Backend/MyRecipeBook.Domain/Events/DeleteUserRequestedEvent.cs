namespace MyRecipeBook.Domain.Events;

public class DeleteUserRequestedEvent
{
    public required Guid UserIdentifier { get; set; }
}
