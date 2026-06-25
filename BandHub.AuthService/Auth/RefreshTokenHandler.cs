using BandHub.AuthService.Domain;
using BandHub.AuthService.Features.Login;

namespace BandHub.AuthService.Auth;

public class RefreshTokenHandler
{
    private readonly ITokenService _tokenService;
    private readonly IAccountAuthRepository _accountAuthRepository;

    public RefreshTokenHandler(ITokenService tokenService, IAccountAuthRepository accountAuthRepository)
    {
        _tokenService = tokenService;
        _accountAuthRepository = accountAuthRepository;
    }

    public async Task<LoginResponse> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var account = await _accountAuthRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (account is null || account.RefreshTokenExpiraEm < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var accessToken = _tokenService.GerarAcessToken(account);
        var refreshToken = _tokenService.GerarRefreshToken();

        await _accountAuthRepository.UpdateRefreshTokenAsync(account.Id, refreshToken, cancellationToken);

        return new LoginResponse(
            account.Id,
            account.Name,
            account.Email,
            account.AccountType.ToString(),
            accessToken,
            DateTime.UtcNow.AddMinutes(15),
            refreshToken,
            DateTime.UtcNow.AddDays(7));
    }
}
