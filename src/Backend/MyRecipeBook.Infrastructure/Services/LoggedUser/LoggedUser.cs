using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Domain.Services.LoggedUser;
using MyRecipeBook.Infrastructure.DataAccess;

namespace MyRecipeBook.Infrastructure.Services.LoggedUser;

public class LoggedUser : ILoggedUser
{
    private readonly MyRecipeBookDbContext _dbContext;
    private readonly ITokenProvider _tokenProvider;

    public LoggedUser(MyRecipeBookDbContext dbContext, ITokenProvider tokenProvider)
    {
        _dbContext = dbContext;
        _tokenProvider = tokenProvider;
    }

    public async Task<User> User()
    {
        var userIdentifier = _tokenProvider.UserIdentifier();

        return await _dbContext
            .Users
            .AsNoTracking()
            .FirstAsync(user => user.Active && user.UserIdentifier == userIdentifier);
    }
}
