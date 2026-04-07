namespace MyRecipeBook.Domain.Security.Tokens;
public interface ITokenProvider
{
    public Guid UserIdentifier();
}
