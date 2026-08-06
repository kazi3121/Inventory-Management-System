using AutoMapper;
using IMS.Application.Common.Interfaces;
using IMS.Application.DTOs;
using MediatR;

namespace IMS.Application.Features.Products.Queries;

public record GetPagedProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    int? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null
) : IRequest<PagedResultDto<ProductDto>>;

public class GetPagedProductsQueryHandler : IRequestHandler<GetPagedProductsQuery, PagedResultDto<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetPagedProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ProductDto>> Handle(GetPagedProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _productRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<ProductDto>>(items);

        return new PagedResultDto<ProductDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}