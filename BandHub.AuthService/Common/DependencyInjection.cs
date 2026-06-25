using BandHub.AuthService.Auth;
using BandHub.AuthService.Domain;
using BandHub.AuthService.Features.Login;
using BandHub.AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BandHub.AuthService.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AccountAuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAccountAuthRepository, AccountAuthRepository>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshTokenHandler>();

        return services;
    }
}
