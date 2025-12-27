using AutoMapper;
using ProductService.BLL.Extensions;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Category;
using ProductService.BLL.Models.Product;
using ProductService.DAL.Models;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using UserService.Domain.Entities;

namespace ProductService.BLL.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, ProductCategoryModel>();

        CreateMap<User, SellerModel>();

        CreateMap<Product, ProductModel>()
            .ForMember(destination => destination.ImageUrls, option => option.MapFrom(source => source.Images.Select(image => image.Url)));

        CreateMap<CreateProductModel, Product>()
            .ForMember(destination => destination.Priority, option => option.MapFrom(source => Priority.Low))
            .ForMember(destination => destination.Status, option => option.MapFrom(source => ProductStatus.Available))
            .ForMember(destination => destination.Images, option => option.MapFrom(source =>
                source.ImageUrls != null
                    ? source.ImageUrls.Select(url => new ProductImage { Url = url }).ToList()
                    : new List<ProductImage>()));

        CreateMap<UpdateProductModel, Product>();

        CreateMap(typeof(PagedList<>), typeof(PagedResult<>))
            .ForMember(nameof(PagedResult<object>.ContinuationToken),
                option => option.MapFrom(nameof(PagedList<object>.LastId)));

        CreateMap<Category, CategoryModel>();

        CreateMap<CreateCategoryModel, Category>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.ToSentenceCase()));

        CreateMap<CategoryModel, Category>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.ToSentenceCase()));
    }
}
