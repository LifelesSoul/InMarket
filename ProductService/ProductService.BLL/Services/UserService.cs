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
    public async Task<IList<UserModel>> GetAll(int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            page = PaginationConstants.DefaultPageNumber;
        }
        if (pageSize < 1)
        {
            pageSize = PaginationConstants.DefaultPageSize;
        }

        var entities = await repository.GetPaged(page, pageSize, cancellationToken);

        return mapper.Map<IList<UserModel>>(entities);
    }

    public async Task<UserModel?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(id, cancellationToken, disableTracking: true);

        if (entity is null)
        {
            return null;
        }

        return mapper.Map<UserModel>(entity);
    }

    public async Task<UserModel> Create(CreateUserModel model, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(model, cancellationToken);

        var existingUser = await repository.GetByEmail(model.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new InvalidOperationException($"User with email {model.Email} already exists.");
        }

        var entity = mapper.Map<User>(model);

        entity.Profile = new UserProfile
        {
            UserId = entity.Id,
            User = entity,
            RatingScore = 1
        };

        if (model.Role == UserRoles.None)
        {
            entity.Role = UserRolePresets.BuyerOnly;
        }

        await repository.Add(entity, cancellationToken);

        return mapper.Map<UserModel>(entity);
    }

    public async Task<UserModel> Update(UpdateUserModel model, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(model, cancellationToken);

        var entity = await repository.GetById(model.Id, cancellationToken, disableTracking: false)
            ?? throw new KeyNotFoundException($"User {model.Id} not found");

        if (string.IsNullOrWhiteSpace(model.Email) && model.Email != entity.Email)
        {
            var emailTaken = await repository.GetByEmail(model.Email, cancellationToken);
            if (emailTaken is not null)
            {
                throw new InvalidOperationException("Email is taken");
            }
            entity.Email = model.Email;
        }

        await repository.Update(entity, cancellationToken);

        return mapper.Map<UserModel>(entity);
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(id, cancellationToken, disableTracking: false)
            ?? throw new KeyNotFoundException($"User {id} not found");

        await repository.Delete(entity, cancellationToken);
    }

    public async Task SyncUserAsync(string externalId, string email, CancellationToken cancellationToken)
    {
        var user = await repository.GetByExternalId(externalId, cancellationToken);

        if (user is null)
        {
            user = await repository.GetByEmail(email, cancellationToken);
            if (user is not null && string.IsNullOrEmpty(user.ExternalId))
            {
                user.ExternalId = externalId;
                await repository.Update(user, cancellationToken);
                return;
            }

            var syncModel = new Auth0SyncModel
            {
                ExternalId = externalId,
                Email = email
            };

            var newUser = mapper.Map<User>(syncModel);

            newUser.Profile = new UserProfile
            {
                UserId = newUser.Id,
                User = newUser,
                RatingScore = 0
            };

            await repository.Add(newUser, cancellationToken);
        }
    }

}

public interface IUserService
{
    Task SyncUserAsync(string externalId, string email, CancellationToken cancellationToken);
    Task<IList<UserModel>> GetAll(int page, int pageSize, CancellationToken cancellationToken);
    Task<UserModel?> GetById(Guid id, CancellationToken cancellationToken);
    Task<UserModel> Create(CreateUserModel model, CancellationToken cancellationToken);
    Task<UserModel> Update(UpdateUserModel model, CancellationToken cancellationToken);
    Task Delete(Guid id, CancellationToken cancellationToken);
}
