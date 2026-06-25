using BandHub.AuthService.Domain;

namespace BandHub.AuthService.Auth;

public interface ITokenService
{
    string GerarAcessToken(Account account);
    string GerarRefreshToken();
}
