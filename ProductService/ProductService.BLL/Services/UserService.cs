using AutoMapper;
using FluentValidation;
using ProductService.BLL.Models.User;
using ProductService.DAL.Repositories;
using ProductService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace ProductService.BLL.Services;

public class UsersService(
    IUserRepository repository,
    IMapper mapper,
    IValidator<CreateUserModel> createValidator,
    IValidator<UpdateUserModel> updateValidator
    ) : IUserService
{
    public async Task<IReadOnlyList<UserModel>> GetAll(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = PaginationConstants.DefaultPageNumber;
        if (pageSize < 1) pageSize = PaginationConstants.DefaultPageSize;

        var entities = await repository.GetPaged(page, pageSize, cancellationToken);

        return mapper.Map<IReadOnlyList<UserModel>>(entities);
    }

    public async Task<UserModel?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetById(id, disableTracking: true, cancellationToken);

        if (entity == null) return null;

        return mapper.Map<UserModel>(entity);
    }

    public async Task<UserModel> Create(CreateUserModel model, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(model, cancellationToken);

        var existingUser = await repository.GetByEmail(model.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email {model.Email} already exists.");
        }

        var entity = mapper.Map<User>(model);

        entity.PasswordHash = model.Password;

        entity.Profile = new UserProfile
        {
            UserId = entity.Id,
            User = entity,
            RatingScore = 1,
            Biography = null,
            AvatarUrl = null
        };

        if (model.Role == UserRole.None)
        {
            entity.Role = UserRoles.BuyerOnly;
        }

        await repository.Add(entity, cancellationToken);

        return mapper.Map<UserModel>(entity);
    }

    public async Task<UserModel> Update(UpdateUserModel model, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(model, cancellationToken);

        var entity = await repository.GetById(model.Id, disableTracking: false, cancellationToken)
                     ?? throw new KeyNotFoundException($"User {model.Id} not found");

        if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != entity.Email)
        {
            var emailTaken = await repository.GetByEmail(model.Email, cancellationToken);
            if (emailTaken != null) throw new InvalidOperationException("Email is taken");
            entity.Email = model.Email;
        }

        await repository.Update(entity, cancellationToken);

        return mapper.Map<UserModel>(entity);
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetById(id, disableTracking: false, cancellationToken)
            ?? throw new KeyNotFoundException($"User {id} not found");

        await repository.Delete(entity, cancellationToken);
    }
}

public interface IUserService
{
    Task<IReadOnlyList<UserModel>> GetAll(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<UserModel?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<UserModel> Create(CreateUserModel model, CancellationToken cancellationToken = default);
    Task<UserModel> Update(UpdateUserModel model, CancellationToken cancellationToken = default);
    Task Delete(Guid id, CancellationToken cancellationToken = default);
}
