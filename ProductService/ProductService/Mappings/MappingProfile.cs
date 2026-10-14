using AutoMapper;
using ProductService.API.Models;
using ProductService.API.ViewModels.User;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Category;
using ProductService.BLL.Models.Product;
using ProductService.BLL.Models.User;

namespace ProductService.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SellerModel, SellerViewModel>();

        CreateMap<ProductModel, ProductViewModel>()
            .ForMember(destination => destination.CategoryId, option => option.MapFrom(source => source.Category.Id))
            .ForMember(destination => destination.CategoryName, option => option.MapFrom(source => source.Category.Name))
            .ForMember(destination => destination.ImageUrl, option => option.MapFrom(source => source.ImageUrls.FirstOrDefault()))
            .ForMember(destination => destination.CreatedAt, option => option.MapFrom(source => source.CreationDate));

        CreateMap(typeof(PagedResult<>), typeof(PagedResult<>));

        CreateMap<UserModel, UserViewModel>();

        CreateMap<CategoryModel, CategoryViewModel>();
    }
}
