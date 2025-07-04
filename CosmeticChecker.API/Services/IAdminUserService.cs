using CosmeticChecker.API.DTOs;
using System.Security.Claims;

public interface IAdminUserService
{
    Task<List<AdminUserDto>> GetAllUsersAsync();
    Task<string> ChangeUserRoleAsync(int userId, ChangeRoleRequest request);
    Task<string> DeleteUserAsync(int userId, ClaimsPrincipal currentUser);
}
