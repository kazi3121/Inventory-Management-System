using IMS.Domain.Entities;
using IMS.Domain.Enums;

namespace IMS.Application.DTOs;

public record RegisterRequestDto(
    string Username,
    string Email,
    string Password,
    UserRole Role = UserRole.Staff
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    int Id,
    string Username,
    string Email,
    string Role,
    string Token
);