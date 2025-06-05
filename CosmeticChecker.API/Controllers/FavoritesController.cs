using CosmeticChecker.API.DTOs;
using CosmeticChecker.API.Services;
using DatabaseContext;
using DatabaseModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/favorites")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoritesService _favoritesService;
        private readonly ILogger<FavoritesController> _logger;

        public FavoritesController(IFavoritesService favoritesService, ILogger<FavoritesController> logger)
        {
            _favoritesService = favoritesService;
            _logger = logger;
        }

        // Получить избранные продукты пользователя с пагинацией
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FavoriteProductDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<FavoriteProductDto>>> GetUserFavorites(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized("Требуется авторизация");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var favoriteProducts = await _favoritesService.GetUserFavoritesAsync(userId, page, pageSize);

            return Ok(favoriteProducts);
        }

        // Добавить продукт в избранное
        [HttpPost("{productId}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddToFavorites(int productId)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized("Требуется авторизация");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _favoritesService.AddToFavoritesAsync(userId, productId);

            if (result != null)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetUserFavorites), new { page = 1, pageSize = 10 }, null);
        }

        // Удалить продукт из избранного
        [HttpDelete("{productId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveFromFavorites(int productId)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized("Требуется авторизация");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _favoritesService.RemoveFromFavoritesAsync(userId, productId);

            if (result != null)
                return BadRequest(result);

            return NoContent();
        }
    }

}
