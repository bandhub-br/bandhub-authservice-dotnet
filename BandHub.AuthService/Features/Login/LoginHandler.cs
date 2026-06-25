using BandHub.AuthService.Auth;
using BandHub.AuthService.Domain;
using Microsoft.AspNetCore.Http;

namespace BandHub.AuthService.Features.Login;

public class LoginHandler
{
    private readonly IAccountAuthRepository _accountAuthRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginHandler(
        IAccountAuthRepository accountAuthRepository, 
        ITokenService tokenService, 
        ILogger<LoginHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _accountAuthRepository = accountAuthRepository;
        _tokenService = tokenService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        _logger.LogInformation("Login attempt for email {Email} from IP {IP}", request.Email, ip);
        var account = await _accountAuthRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Login failed for email {Email} from IP {IP}: account not found", request.Email, ip);
            throw new InvalidOperationException("Credencias Inválidas.");
        }

        if (account.PasswordHash != request.Password) // Temporário: depois trocamos por hash real
        {
            _logger.LogWarning("Login failed for email {Email} from IP {IP}: invalid password", request.Email, ip);
            throw new InvalidOperationException("Credencias Inválidas.");
        }

        var accessToken = _tokenService.GerarAcessToken(account);
        var refreshToken = _tokenService.GerarRefreshToken();

        await _accountAuthRepository.UpdateRefreshTokenAsync(account.Id, refreshToken, cancellationToken);

        _logger.LogInformation("Login successful for email {Email} from IP {IP}", request.Email, ip);

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
