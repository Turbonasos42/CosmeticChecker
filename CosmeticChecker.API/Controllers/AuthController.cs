using CosmeticChecker.API.DTOs;
using CosmeticChecker.API.DTOs;
using CosmeticChecker.API.Services;
using DatabaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using System.Security.Claims;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result.IsSuccess)
            return Created(string.Empty, new { result.User.Id, result.User.Email, result.User.FirstName, result.User.LastName });

        return BadRequest(new { Message = result.Message });
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.IsSuccess)
            return Ok(result.User);

        return Unauthorized(new { Message = result.Message });
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(new { Message = "Выход выполнен успешно" });
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(nameIdentifier))
        {
            return Unauthorized(new { Message = "Пользователь не аутентифицирован или отсутствует NameIdentifier." });
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Передаем идентификатор текущего пользователя в сервис для получения его данных
        var userProfile = await _authService.GetCurrentUserAsync(currentUserId);

        if (userProfile == null)
            return Unauthorized(new { Message = "Пользователь не найден" });

        return Ok(userProfile);
    }

    [HttpPatch("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)] // Успешный ответ без содержимого
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Ошибка при валидации данных
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Ошибка при отсутствии авторизации
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
    {
        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(nameIdentifier))
        {
            return Unauthorized(new { Message = "Пользователь не аутентифицирован" });
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Передаем запрос на обновление данных пользователя в сервис
        var result = await _authService.UpdateCurrentUserAsync(currentUserId, request);

        if (!result.IsSuccess)
            return BadRequest(new { Message = result.Message });

        return NoContent(); // Возвращаем статус 204 NoContent при успешном обновлении
    }
}
