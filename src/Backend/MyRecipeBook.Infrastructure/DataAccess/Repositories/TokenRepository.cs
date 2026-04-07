using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories.Token;

namespace MyRecipeBook.Infrastructure.DataAccess.Repositories;
public class TokenRepository : ITokenRepository
{
    private readonly MyRecipeBookDbContext _dbContext;

    public TokenRepository(MyRecipeBookDbContext dbContext) => _dbContext = dbContext;

    public async Task<RefreshToken?> Get(string refreshToken)
    {
        return await _dbContext
            .RefreshTokens
            .AsNoTracking()
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.Value.Equals(refreshToken));
    }

    public async Task SaveNewRefreshToken(RefreshToken refreshToken)
    {
        var activeTokens = _dbContext
            .RefreshTokens
            .Where(token => token.UserId == refreshToken.UserId && token.RevokedAt == null);

        var revokedAt = DateTime.UtcNow;

        await activeTokens.ForEachAsync(token => token.RevokedAt = revokedAt);

        await _dbContext.RefreshTokens.AddAsync(refreshToken);
    }
}
