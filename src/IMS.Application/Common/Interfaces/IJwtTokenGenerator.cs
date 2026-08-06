using IMS.Domain.Entities;

namespace IMS.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}