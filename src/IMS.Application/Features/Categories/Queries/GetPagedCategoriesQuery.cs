using AutoMapper;
using IMS.Application.Common.Interfaces;
using IMS.Application.DTOs;
using MediatR;

namespace IMS.Application.Features.Categories.Queries;

public record GetPagedCategoriesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null
) : IRequest<PagedResultDto<CategoryDto>>;

public class GetPagedCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    : IRequestHandler<GetPagedCategoriesQuery, PagedResultDto<CategoryDto>>
{
    public async Task<PagedResultDto<CategoryDto>> Handle(GetPagedCategoriesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await categoryRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            cancellationToken);

        var dtos = mapper.Map<IReadOnlyList<CategoryDto>>(items);

        return new PagedResultDto<CategoryDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}