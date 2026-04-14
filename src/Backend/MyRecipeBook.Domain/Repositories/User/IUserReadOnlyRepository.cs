namespace MyRecipeBook.Domain.Repositories.User;
public interface IUserReadOnlyRepository
{
    public Task<bool> ExistActiveUserWithEmail(MyRecipeBook.Domain.ValueObjects.Email email);
    public Task<bool> ExistActiveUserWithIdentifier(Guid userIdentifier);
    public Task<bool> ExistUserWithIdentifier(Guid userIdentifier);
    public Task<Entities.User?> GetByEmail(MyRecipeBook.Domain.ValueObjects.Email email);
}
