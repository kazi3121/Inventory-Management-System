using FluentAssertions;
using IMS.Application.Common.Interfaces;
using IMS.Application.DTOs;
using IMS.Application.Features.Auth.Commands;
using IMS.Domain.Entities;
using IMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace IMS.UnitTestS.Auth;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<ILogger<LoginUserCommandHandler>> _loggerMock;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _loggerMock = new Mock<ILogger<LoginUserCommandHandler>>();

        _handler = new LoginUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnAuthResponseDto_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginUserCommand("john@example.com", "Password123!");
        var user = new User
        {
            Id = 1,
            Username = "john_doe",
            Email = command.Email,
            PasswordHash = "hashed_secret_password",
            Role = UserRole.Admin
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(user))
            .Returns("valid_jwt_token");

        // Act
        AuthResponseDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be(user.Role.ToString());
        result.Token.Should().Be("valid_jwt_token");
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessException_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new LoginUserCommand("nonexistent@example.com", "Password123!");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");

        _passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var command = new LoginUserCommand("john@example.com", "WrongPassword!");
        var user = new User
        {
            Id = 1,
            Username = "john_doe",
            Email = command.Email,
            PasswordHash = "hashed_secret_password",
            Role = UserRole.Admin
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");

        _jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }
}