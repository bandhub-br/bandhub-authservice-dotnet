using BandHub.AuthService.Domain;
using System.Security.Claims;

namespace BandHub.AuthService.Auth
{
    public static class LogoutEndpoint
    {
        public static IEndpointRouteBuilder MapLogoutEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/logout", async (
            IAccountAuthRepository repo,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
            {
                var userId = httpContext.User
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                await repo.RevokeRefreshTokenAsync(
                    Guid.Parse(userId), cancellationToken);

                return Results.NoContent();
            })
            .WithName("Logout")
           .WithTags("Auth")
           .RequireAuthorization();  

            return app;
        }
    }
}
