namespace BandHub.AuthService.Features.Login;

public sealed record LoginResponse(
    Guid AccountId,
    string Name,
    string Email,
    string AccountType,
    string AcessToken,
    DateTime AcessTokenExpiraEm,
    string RefreshToken,
    DateTime RefreshTokenExpiraEm);
