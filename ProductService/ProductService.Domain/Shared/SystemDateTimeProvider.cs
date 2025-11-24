namespace ProductService.Domain.Shared;
public static class SystemDateTimeProvider
{
    public static DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
