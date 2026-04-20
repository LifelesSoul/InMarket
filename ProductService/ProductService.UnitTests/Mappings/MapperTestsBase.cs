using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace ProductService.Tests.Mappings;

public abstract class MapperTestsBase<TProfile> where TProfile : Profile, new()
{
    protected readonly IMapper Mapper;

    protected MapperTestsBase()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<TProfile>(),
            NullLoggerFactory.Instance
        );

        Mapper = config.CreateMapper();
    }
}