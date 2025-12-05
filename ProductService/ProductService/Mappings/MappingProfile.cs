using AutoMapper;
using ProductService.API.ViewModels.User;
using ProductService.BLL.Models;
using ProductService.BLL.Models.Product;

namespace ProductService.Mappings;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<SellerModel, SellerViewModel>();

        CreateMap<ProductModel, ProductViewModel>()
            .ForMember(destination => destination.CategoryName, option => option.MapFrom(source => source.Category.Name))
            .ForMember(destination => destination.ImageUrl, option => option.MapFrom(source => source.ImageUrls.FirstOrDefault()));

        CreateMap(typeof(PagedResult<>), typeof(PagedResult<>));
    }
}
