using AutoMapper;
using IMS.Application.DTOs;
using IMS.Domain.Entities;

namespace IMS.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, AuthResponseDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.Token, opt => opt.Ignore()); // Token set dynamically in Handler

        CreateMap<Category, CategoryDto>();

        // Product Mapping with Category Name Flattening
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
    }
}