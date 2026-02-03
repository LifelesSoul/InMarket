using AutoMapper;
using ProductService.BLL.Events;
using ProductService.BLL.Extensions;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Category;
using ProductService.BLL.Models.Product;
using ProductService.BLL.Models.User;
using ProductService.DAL.Models;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

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

        CreateMap<User, SellerModel>();

        CreateMap<User, UserModel>()
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.AvatarUrl : null))
            .ForMember(dest => dest.Biography, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.Biography : null))
            .ForMember(dest => dest.RatingScore, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.RatingScore : 0));

        CreateMap<CreateUserModel, User>();

        CreateMap<Category, CategoryModel>();

        CreateMap<CreateCategoryModel, Category>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.ToSentenceCase()));

        CreateMap<CategoryModel, Category>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.ToSentenceCase()));

        CreateMap<Auth0SyncModel, User>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Email.Substring(0, src.Email.IndexOf('@'))))
            .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => TimeProvider.System.GetUtcNow()))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => UserRoles.Buyer));

        CreateMap<Product, CreateNotificationEvent>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.SellerId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom((src, dest, destMember, context) =>
                context.Items[nameof(CreateNotificationEvent.Title)]))
            .ForMember(dest => dest.Message, opt => opt.MapFrom((src, dest, destMember, context) =>
                context.Items[nameof(CreateNotificationEvent.Message)]));
    }
}
