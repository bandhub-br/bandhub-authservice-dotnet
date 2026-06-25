using BandHub.AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace BandHub.AuthService.Infrastructure.Persistence;

public class AccountAuthDbContext : DbContext
{
    public AccountAuthDbContext(DbContextOptions<AccountAuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.AccountType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.RefreshToken)
                .HasMaxLength(500);

            entity.Property(x => x.RefreshTokenExpiraEm);
        });
    }
}
