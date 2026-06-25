using BandHub.AuthService.Auth;
using BandHub.AuthService.Domain;
using BandHub.AuthService.Features.Login;
using FluentAssertions;
using Moq;

namespace BandHub.AuthService.UnitTests.Features.Login;

public class LoginHandlerTests
{
    private readonly Mock<IAccountAuthRepository> _accountRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _accountRepositoryMock = new Mock<IAccountAuthRepository>();
        _tokenServiceMock = new Mock<ITokenService>();

        _tokenServiceMock
            .Setup(x => x.GerarAcessToken(It.IsAny<Account>()))
            .Returns("fake-access-token");

        _tokenServiceMock
            .Setup(x => x.GerarRefreshToken())
            .Returns("fake-refresh-token");

        _handler = new LoginHandler(_accountRepositoryMock.Object, _tokenServiceMock.Object);
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
            PasswordHash = "password123",
            AccountType = AccountType.User,
            CreatedAt = DateTime.UtcNow
        };

        _accountRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _accountRepositoryMock
            .Setup(x => x.UpdateRefreshTokenAsync(account.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _handler.HandleAsync(request, CancellationToken.None);

        response.AccountId.Should().Be(account.Id);
        response.Email.Should().Be(account.Email);
        response.Name.Should().Be(account.Name);
        response.AccountType.Should().Be(AccountType.User.ToString());
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
            PasswordHash = "password123",
            AccountType = AccountType.User,
            CreatedAt = DateTime.UtcNow
        };

        _accountRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Credencias Inválidas.");
    }
}
