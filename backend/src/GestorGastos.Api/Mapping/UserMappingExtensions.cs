using GestorGastos.Api.Dtos.Auth;
using GestorGastos.Domain.Entities;

namespace GestorGastos.Api.Mapping;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user) =>
        new(user.Id, user.Email, user.DisplayName, user.TwoFactorEnabled, user.TwoFactorEnabledAt);
}
