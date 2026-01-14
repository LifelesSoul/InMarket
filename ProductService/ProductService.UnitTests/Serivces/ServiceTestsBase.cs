using AutoMapper;
using Moq;

namespace ProductService.Tests.Services;

public abstract class ServiceTestsBase
{
    protected readonly Mock<IMapper> MapperMock;
    protected readonly CancellationToken Ct;

    protected ServiceTestsBase()
    {
        MapperMock = new Mock<IMapper>();
        Ct = CancellationToken.None;
    }
}
