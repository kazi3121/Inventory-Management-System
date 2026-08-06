using IMS.Application.DTOs;
using IMS.Application.Features.Categories.Commands;
using IMS.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// GET /api/categories?pageNumber=1&pageSize=10&searchTerm=electronics
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<CategoryDto>>> GetPaged([FromQuery] GetPagedCategoriesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var result = await mediator.Send(new GetCategoryByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequestDto request)
    {
        var command = new CreateCategoryCommand(request.Name, request.Description);
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] UpdateCategoryRequestDto request)
    {
        var command = new UpdateCategoryCommand(id, request.Name, request.Description);
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteCategoryCommand(id));
        return NoContent();
    }
}