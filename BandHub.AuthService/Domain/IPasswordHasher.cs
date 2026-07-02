namespace BandHub.UserService.Features.Accounts.Domain
{
    public interface IPasswordHasher
    {
        string HasPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
