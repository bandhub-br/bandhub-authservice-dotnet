using BandHub.AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace BandHub.AuthService.Infrastructure.Persistence;

public class AccountAuthRepository : IAccountAuthRepository
{
    private readonly AccountAuthDbContext _context;

    public AccountAuthRepository(AccountAuthDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<Account?> GetByRefreshTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(x => x.RefreshToken == token, cancellationToken);
    }

    public async Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken)
    {
        await _context.Accounts
            .Where(x => x.Id == userId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(a => a.RefreshToken, refreshToken)
                .SetProperty(a => a.RefreshTokenExpiraEm, DateTime.UtcNow.AddDays(7)),
                cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _context.Accounts
        .Where(x => x.Id == userId)
        .ExecuteUpdateAsync(x => x
            .SetProperty(a => a.RefreshToken, (string?)null)
            .SetProperty(a => a.RefreshTokenExpiraEm, (DateTime?)null),
            cancellationToken);
    }
}
