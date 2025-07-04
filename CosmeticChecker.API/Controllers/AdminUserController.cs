using CosmeticChecker.API.DTOs;
using DatabaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using CosmeticChecker.API.Services;

[Authorize(Policy = "AdminOnly")]
[Route("api/admin/users")]
[ApiController]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        IAdminUserService adminUserService,
        ILogger<AdminUsersController> logger)
    {
        _adminUserService = adminUserService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AdminUserDto>), 200)]
    public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetAllUsers()
    {
        var users = await _adminUserService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut("{userId}/role")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ChangeUserRole(int userId, [FromBody] ChangeRoleRequest request)
    {
        var result = await _adminUserService.ChangeUserRoleAsync(userId, request);

        if (result == null)
            return NotFound("Пользователь или роль не найдены");

        if (result == "Cannot change main admin")
            return BadRequest("Нельзя изменить роль главного администратора");

        _logger.LogInformation($"User {User.Identity.Name} changed role for user {userId}");

        return NoContent();
    }

    [HttpDelete("{userId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var currentUser = User; // Получаем текущего пользователя из контекста
        var result = await _adminUserService.DeleteUserAsync(userId, currentUser);

        if (result == null)
            return NotFound("Пользователь не найден");

        if (result == "Cannot delete main admin")
            return BadRequest("Нельзя удалить главного администратора");

        if (result == "Cannot delete yourself")
            return BadRequest("Нельзя удалить самого себя");

        _logger.LogWarning($"User {User.Identity.Name} deleted user {userId}");

        return NoContent();
    }

}
