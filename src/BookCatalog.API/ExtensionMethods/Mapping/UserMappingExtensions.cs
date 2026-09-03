using BookCatalog.API.Dtos.User;
using BookCatalog.API.Entities;

namespace BookCatalog.API.ExtensionMethods.Mapping;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }

    public static User ToEntity(this UpsertUserRequestDto request)
    {
        return new User
        {
            Name = request.Name,
            Email = request.Email
        };
    }
}
