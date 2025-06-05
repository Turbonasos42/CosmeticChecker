using CosmeticChecker.API.DTOs;
using System.Security.Claims;

namespace CosmeticChecker.API.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(UserRegistrationDto request); // Регистрация пользователя
        Task<AuthResult> LoginAsync(UserLoginDto request);           // Аутентификация (вход)
        Task LogoutAsync();                                          // Выход пользователя
        Task<UserProfileDto> GetCurrentUserAsync(int UserId);                  // Получение данных текущего пользователя
        Task<AuthResult> UpdateCurrentUserAsync(int userId, UpdateUserRequest request);
    }


}
