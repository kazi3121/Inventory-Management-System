using IMS.Application.Common.Interfaces;
using IMS.Application.DTOs;
using IMS.Domain.Enums;
using IMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IMS.Application.Features.Auth.Commands;

public record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    UserRole Role
) : IRequest<AuthResponseDto>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

        bool isUnique = await _userRepository.IsEmailUniqueAsync(request.Email, cancellationToken);
        if (!isUnique)
        {
            _logger.LogWarning("Registration failed. Email {Email} is already registered.", request.Email);
            throw new InvalidOperationException($"User with email '{request.Email}' already exists.");
        }

        string passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = request.Role
        };

        await _userRepository.AddAsync(user, cancellationToken);
        _logger.LogInformation("User successfully created with ID: {UserId}", user.Id);

        string token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString(),
            token
        );
    }
}