namespace IMS.Application.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string Description,
    DateTime CreatedAt
);

public record CreateCategoryRequestDto(
    string Name,
    string Description
);

public record UpdateCategoryRequestDto(
    string Name,
    string Description
);