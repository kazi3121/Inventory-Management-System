using FluentAssertions;
using IMS.Application.Common.Interfaces;
using IMS.Application.DTOs;
using IMS.Application.Features.Auth.Commands;
using IMS.Domain.Entities;
using IMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IMS.UnitTestS.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<ILogger<RegisterUserCommandHandler>> _loggerMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _loggerMock = new Mock<ILogger<RegisterUserCommandHandler>>();

        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_RegisterUserAndReturnAuthResponseDto_WhenEmailIsUnique()
    {
        // Arrange
        var command = new RegisterUserCommand("john_doe", "john@example.com", "Password123!", UserRole.Admin);

        _userRepositoryMock
            .Setup(x => x.IsEmailUniqueAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _passwordHasherMock
            .Setup(x => x.HashPassword(command.Password))
            .Returns("hashed_secret_password");

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => user.Id = 1);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("generated_jwt_token");

        // Act
        AuthResponseDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Username.Should().Be(command.Username);
        result.Email.Should().Be(command.Email);
        result.Role.Should().Be(command.Role.ToString());
        result.Token.Should().Be("generated_jwt_token");

        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.Username == command.Username &&
            u.Email == command.Email &&
            u.PasswordHash == "hashed_secret_password" &&
            u.Role == command.Role
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenEmailIsNotUnique()
    {
        // Arrange
        var command = new RegisterUserCommand("john_doe", "existing@example.com", "Password123!", UserRole.Staff);

        _userRepositoryMock
            .Setup(x => x.IsEmailUniqueAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User with email '{command.Email}' already exists.");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _jwtTokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }
}