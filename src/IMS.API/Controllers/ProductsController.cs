using IMS.Application.DTOs;
using IMS.Application.Features.Products.Commands;
using IMS.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// GET /api/products?pageNumber=1&pageSize=10&searchTerm=laptop&categoryId=2&minPrice=100&maxPrice=1500
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetPaged([FromQuery] GetPagedProductsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequestDto request)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.SKU,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.CategoryId
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductRequestDto request)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.SKU,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.CategoryId
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }
}