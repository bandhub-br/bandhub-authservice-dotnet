using BandHub.AuthService.Auth;
using BandHub.AuthService.Domain;

namespace BandHub.AuthService.Features.Login;

public class LoginHandler
{
    private readonly IAccountAuthRepository _accountAuthRepository;
    private readonly ITokenService _tokenService;

    public LoginHandler(IAccountAuthRepository accountAuthRepository, ITokenService tokenService)
    {
        _accountAuthRepository = accountAuthRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var account = await _accountAuthRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (account == null)
            throw new InvalidOperationException("Credencias Inválidas.");

        if (account.PasswordHash != request.Password) // Temporário: depois trocamos por hash real
            throw new InvalidOperationException("Credencias Inválidas.");

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
