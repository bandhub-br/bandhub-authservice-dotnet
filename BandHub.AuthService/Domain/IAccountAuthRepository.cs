namespace BandHub.AuthService.Domain;

public interface IAccountAuthRepository
{
    Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Account?> GetByRefreshTokenAsync(string token, CancellationToken cancellationToken);
    Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken);
}
