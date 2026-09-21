using AutoMapper;
using ProductService.BLL.Models.Profile;
using ProductService.DAL.Repositories;

namespace ProductService.BLL.Services;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IMapper _mapper;

    public ProfileService(IProfileRepository profileRepository, IMapper mapper)
    {
        _profileRepository = profileRepository;
        _mapper = mapper;
    }

    public async Task<UserProfileDto?> GetProfileByExternalIdAsync(string externalId)
    {
        var userEntity = await _profileRepository.GetUserWithProfileAsync(externalId);

        if (userEntity == null) return null;

        return _mapper.Map<UserProfileDto>(userEntity);
    }
}
public interface IProfileService
{
    Task<UserProfileDto?> GetProfileByExternalIdAsync(string externalId);
}
