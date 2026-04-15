using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories.Token;

namespace MyRecipeBook.Infrastructure.DataAccess.Repositories;
public class TokenRepository : ITokenRepository
{
    private readonly MyRecipeBookDbContext _dbContext;

    public TokenRepository(MyRecipeBookDbContext dbContext) => _dbContext = dbContext;

    public async Task<RefreshToken?> Get(string refreshToken) =>
        await _dbContext
            .RefreshTokens
            .AsNoTracking()
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.Value == refreshToken && token.User.Active);

    public async Task SaveNewRefreshToken(RefreshToken refreshToken)
    {
        var activeTokens = await _dbContext
            .RefreshTokens
            .Where(token => token.UserId == refreshToken.UserId && token.RevokedAt == null)
            .ToListAsync();

        var revokedAt = DateTime.UtcNow;

        foreach (var activeToken in activeTokens)
        {
            activeToken.Revoke(revokedAt);
        }

        await _dbContext.RefreshTokens.AddAsync(refreshToken);
    }
}
