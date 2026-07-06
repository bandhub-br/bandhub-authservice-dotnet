using BandHub.AuthService.Auth;
using BandHub.AuthService.Domain;
using BandHub.AuthService.Features.Login;
using BandHub.UserService.Features.Accounts.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace BandHub.AuthService.UnitTests.Features.Login;

public class LoginHandlerTests
{
    private readonly Mock<IAccountAuthRepository> _accountRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ILogger<LoginHandler>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _accountRepositoryMock = new Mock<IAccountAuthRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<LoginHandler>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        _tokenServiceMock
            .Setup(x => x.GerarAcessToken(It.IsAny<Account>()))
            .Returns("fake-access-token");

        _tokenServiceMock
            .Setup(x => x.GerarRefreshToken())
            .Returns("fake-refresh-token");

        _handler = new LoginHandler(
            _accountRepositoryMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _httpContextAccessorMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnLoginResponse_WhenCredentialsAreValid()
    {
        var request = new LoginRequest("john@example.com", "password123");
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "John",
            Email = "john@example.com",
            PasswordHash = "$2a$12$hashedpassword",
            AccountType = 1,
            CreatedAt = DateTime.UtcNow
        };

        _accountRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("password123", account.PasswordHash))
            .Returns(true);

        _accountRepositoryMock
            .Setup(x => x.UpdateRefreshTokenAsync(account.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _handler.HandleAsync(request, CancellationToken.None);

        response.AccountId.Should().Be(account.Id);
        response.Email.Should().Be(account.Email);
        response.Name.Should().Be(account.Name);
        response.AccountType.Should().Be("1");
        response.AcessToken.Should().Be("fake-access-token");
        response.RefreshToken.Should().Be("fake-refresh-token");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenAccountDoesNotExist()
    {
        var request = new LoginRequest("missing@example.com", "password123");

        _accountRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Credencias Inválidas.");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenPasswordIsInvalid()
    {
        var request = new LoginRequest("john@example.com", "wrong-password");
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "John",
            Email = "john@example.com",
            PasswordHash = "$2a$12$hashedpassword",
            AccountType = 1,
            CreatedAt = DateTime.UtcNow
        };

        _accountRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("wrong-password", account.PasswordHash))
            .Returns(false);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Credencias Inválidas.");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCorrectAccountType_ForFan()
    {
        var request = new LoginRequest("fan@example.com", "password123");
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Maria Fan",
            Email = "fan@example.com",
            PasswordHash = "$2a$12$hashedpassword",
            AccountType = 3,
            CreatedAt = DateTime.UtcNow
        };

        _accountRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("password123", account.PasswordHash))
            .Returns(true);

        _accountRepositoryMock
            .Setup(x => x.UpdateRefreshTokenAsync(account.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _handler.HandleAsync(request, CancellationToken.None);

        response.AccountType.Should().Be("3");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCorrectAccountType_ForBand()
    {
        var request = new LoginRequest("band@example.com", "password123");
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "The Midnight Echo",
            Email = "band@example.com",
            PasswordHash = "$2a$12$hashedpassword",
            AccountType = 2,
            CreatedAt = DateTime.UtcNow
        };

        _accountRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("password123", account.PasswordHash))
            .Returns(true);

        _accountRepositoryMock
            .Setup(x => x.UpdateRefreshTokenAsync(account.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _handler.HandleAsync(request, CancellationToken.None);

        response.AccountType.Should().Be("2");
        response.Name.Should().Be("The Midnight Echo");
    }
}
