using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.ValueObjects;

namespace MyRecipeBook.Infrastructure.DataAccess.Repositories;

public class UserRepository : IUserWriteOnlyRepository, IUserReadOnlyRepository, IUserUpdateOnlyRepository, IUserDeleteOnlyRepository
{
    private readonly MyRecipeBookDbContext _dbContext;

    public UserRepository(MyRecipeBookDbContext dbContext) => _dbContext = dbContext;

    public async Task Add(User user) => await _dbContext.Users.AddAsync(user);

    public async Task<bool> ExistActiveUserWithEmail(Email email) => await _dbContext.Users.AnyAsync(user => EF.Property<string>(user, "_email").Equals(email.Value) && user.Active);

    public async Task<bool> ExistActiveUserWithIdentifier(Guid userIdentifier) => await _dbContext.Users.AnyAsync(user => user.UserIdentifier.Equals(userIdentifier) && user.Active);

    public async Task<bool> ExistUserWithIdentifier(Guid userIdentifier) => await _dbContext.Users.AnyAsync(user => user.UserIdentifier.Equals(userIdentifier));

    public async Task<User?> GetById(long id)
    {
        return await _dbContext
            .Users
            .FirstOrDefaultAsync(user => user.Id == id);
    }

    public void Update(User user) => _dbContext.Users.Update(user);

    public async Task DeleteAccount(Guid userIdentifier)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.UserIdentifier == userIdentifier);
        if (user is null)
            return;

        var recipes = _dbContext.Recipes.Where(recipe => recipe.UserId == user.Id);

        _dbContext.Recipes.RemoveRange(recipes);

        _dbContext.Users.Remove(user);
    }

    public async Task<User?> GetByEmail(Email email)
    {
        return await _dbContext
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Active && EF.Property<string>(user, "_email").Equals(email.Value));
    }
}
