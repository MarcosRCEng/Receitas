namespace MyRecipeBook.Domain.Entities;
public class EntityBase
{
    public long Id { get; internal set; }
    public bool Active { get; private set; } = true;
    public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;

    protected void DeactivateEntity()
    {
        Active = false;
    }
}
