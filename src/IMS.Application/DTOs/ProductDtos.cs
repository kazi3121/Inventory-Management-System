namespace IMS.Application.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string SKU,
    string Description,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    string CategoryName,
    DateTime CreatedAt
);

public record CreateProductRequestDto(
    string Name,
    string SKU,
    string Description,
    decimal Price,
    int StockQuantity,
    int CategoryId
);

public record UpdateProductRequestDto(
    string Name,
    string SKU,
    string Description,
    decimal Price,
    int StockQuantity,
    int CategoryId
);