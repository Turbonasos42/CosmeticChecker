using BCrypt.Net;
using CosmeticChecker.API.DTOs;
using CosmeticChecker.API.Services;
using DatabaseModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Repositories;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using static CosmeticChecker.API.Services.IAuthService;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor; // Для получения HttpContext

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;  // Инициализируем IHttpContextAccessor
    }

    // Регистрация пользователя
    public async Task<AuthResult> RegisterAsync(UserRegistrationDto request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
            return new AuthResult { IsSuccess = false, Message = "Email уже занят" };

        await _userRepository.BeginTransactionAsync();

        try
        {
            var userRole = await _roleRepository.GetOrCreateDefaultRoleAsync();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                Password = hashedPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                RoleId = userRole.Id,
                SkinType = request.SkinType,
                Allergies = request.Allergies,
                PreferredIngredients = request.PreferredIngredients,
                DislikedIngredients = request.DislikedIngredients
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            await _userRepository.CommitAsync();

            // Используем HttpContext.SignInAsync для аутентификации
            await SignInUserAsync(user);

            return new AuthResult
            {
                IsSuccess = true,
                User = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };
        }
        catch (Exception ex)
        {
            await _userRepository.RollbackAsync();
            _logger.LogError(ex, "Ошибка при регистрации пользователя");
            return new AuthResult { IsSuccess = false, Message = "Ошибка сервера" };
        }
    }

    // Логин (вход) пользователя
    public async Task<AuthResult> LoginAsync(UserLoginDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return new AuthResult { IsSuccess = false, Message = "Неверные учетные данные" };

        // Используем HttpContext.SignInAsync для аутентификации
        await SignInUserAsync(user);

        return new AuthResult
        {
            IsSuccess = true,
            User = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.Name
            }
        };
    }

    // Выход пользователя
    public async Task LogoutAsync()
    {
        await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);  // Выход через куки
    }

    // Получение данных текущего пользователя
    public async Task<UserProfileDto> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);  // Получаем данные о текущем пользователе

        if (user == null)
            return null;

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.Name,
            SkinType = user.SkinType,
            Allergies = user.Allergies,
            PreferredIngredients = user.PreferredIngredients,
            DislikedIngredients = user.DislikedIngredients
        };
    }

    // Метод для входа через куки
    private async Task SignInUserAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.Name)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
            IsPersistent = true,
            AllowRefresh = true
        };

        await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
    }

    public async Task<AuthResult> UpdateCurrentUserAsync(int userId, UpdateUserRequest request)
    {
        try
        {
            // Получаем пользователя по ID
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return new AuthResult { IsSuccess = false, Message = "Пользователь не найден" };

            // Обновляем поля пользователя, если они были переданы в запросе
            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.Email))
                user.Email = request.Email;

            // Обновление пароля
            if (!string.IsNullOrEmpty(request.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            if (!string.IsNullOrEmpty(request.SkinType))
                user.SkinType = request.SkinType;

            if (!string.IsNullOrEmpty(request.Allergies))
                user.Allergies = request.Allergies;

            if (!string.IsNullOrEmpty(request.PreferredIngredients))
                user.PreferredIngredients = request.PreferredIngredients;

            if (!string.IsNullOrEmpty(request.DislikedIngredients))
                user.DislikedIngredients = request.DislikedIngredients;

            // Сохраняем изменения в базе данных
            await _userRepository.SaveChangesAsync();

            // Возвращаем обновленные данные пользователя
            return new AuthResult
            {
                IsSuccess = true,
                User = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    SkinType = user.SkinType,
                    Allergies = user.Allergies,
                    PreferredIngredients = user.PreferredIngredients,
                    DislikedIngredients = user.DislikedIngredients
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении данных пользователя");
            return new AuthResult { IsSuccess = false, Message = "Ошибка сервера" };
        }
    }


}


