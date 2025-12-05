using AutoMapper;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;
using ProductService.DAL.Models;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using UserService.Domain.Entities;

namespace ProductService.BLL.Mappings;
public class BllMappingProfile : Profile
{
    public BllMappingProfile()
    {
        CreateMap<Category, CategoryModel>();

        CreateMap<User, SellerModel>();

        CreateMap<Product, ProductModel>()
            .ForMember(destination => destination.ImageUrls, option => option.MapFrom(source => source.Images.Select(image => image.Url)))
            .ForMember(destination => destination.CreatedAt, option => option.MapFrom(source => source.CreationDate));

        CreateMap<CreateProductModel, Product>()
            .ForMember(destination => destination.Id, option => option.Ignore())
            .ForMember(destination => destination.CreationDate, option => option.Ignore())
            .ForMember(destination => destination.Images, option => option.Ignore())
            .ForMember(destination => destination.Category, option => option.Ignore())
            .ForMember(destination => destination.Seller, option => option.Ignore())
            .ForMember(destination => destination.Priority, option => option.MapFrom(source => Priority.Low))
            .ForMember(destination => destination.Status, option => option.MapFrom(source => ProductStatus.Available))
            .ForMember(destination => destination.Images, option => option.MapFrom(source =>
                source.ImageUrls != null
                    ? source.ImageUrls.Select(url => new ProductImage { Url = url }).ToList()
                    : new List<ProductImage>()));

        CreateMap<UpdateProductModel, Product>()
            .ForMember(destination => destination.Id, option => option.Ignore())
            .ForMember(destination => destination.CreationDate, option => option.Ignore())
            .ForMember(destination => destination.SellerId, option => option.Ignore())
            .ForMember(destination => destination.Images, option => option.Ignore());

        CreateMap(typeof(PagedList<>), typeof(PagedResult<>))
        .ForMember("ContinuationToken", option => option.MapFrom("LastId"));
    }
}
