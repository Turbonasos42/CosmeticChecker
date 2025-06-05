using CosmeticChecker.API.DTOs;
using DatabaseModels;
using System.Security.Claims;
using Repositories;

namespace CosmeticChecker.API.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(IUserRepository userRepository, IRoleRepository roleRepository, ILogger<AdminUserService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<List<AdminUserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(u => new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                RoleId = u.RoleId,
                RoleName = u.Role.Name
            }).ToList();
        }

        public async Task<string> ChangeUserRoleAsync(int userId, ChangeRoleRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            var role = await _roleRepository.GetRoleByIdAsync(request.RoleId);
            if (role == null)
                return null;

            // Валидация бизнес-логики: не разрешаем изменять роль главного администратора
            if (user.Email == "admin@example.com")
                return "Cannot change main admin";

            user.RoleId = request.RoleId;
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation($"User {userId} changed to role {role.Name}");
            return null;
        }



        public async Task<string> DeleteUserAsync(int userId, ClaimsPrincipal currentUser)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            // Валидация бизнес-логики: не разрешаем удалять главного администратора
            if (user.Email == "admin@example.com")
                return "Cannot delete main admin";

            var currentUserId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user.Id == currentUserId)
                return "Cannot delete yourself";

            _userRepository.RemoveUser(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogWarning($"User {userId} deleted from the system");
            return null;
        }

    }

}


