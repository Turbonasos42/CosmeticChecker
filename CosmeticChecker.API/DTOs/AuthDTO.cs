using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CosmeticChecker.API.DTOs
{
    // DTO для регистрации пользователя
    public class UserRegistrationDto
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
        public string LastName { get; set; }

        [StringLength(100, ErrorMessage = "Тип кожи не должен превышать 100 символов")]
        public string? SkinType { get; set; }

        [StringLength(500, ErrorMessage = "Список аллергенов не должен превышать 500 символов")]
        public string? Allergies { get; set; }

        [StringLength(500, ErrorMessage = "Список предпочитаемых ингредиентов не должен превышать 500 символов")]
        public string? PreferredIngredients { get; set; }

        [StringLength(500, ErrorMessage = "Список нежелательных ингредиентов не должен превышать 500 символов")]
        public string? DislikedIngredients { get; set; }
    }

    // DTO для логина пользователя
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; }
    }

    // DTO для успешного ответа аутентификации
    public class AuthSuccessResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
    }

    // DTO для данных профиля пользователя
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string SkinType { get; set; }
        public string Allergies { get; set; }
        public string PreferredIngredients { get; set; }
        public string DislikedIngredients { get; set; }
    }

    // DTO для обработки ошибок
    public class ErrorResponseDto
    {
        public string Message { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }

    // DTO для обновления данных пользователя
    public class UpdateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } 

        public string? SkinType { get; set; }
        public string? Allergies { get; set; }
        public string? PreferredIngredients { get; set; }
        public string? DislikedIngredients { get; set; }

    }
}
