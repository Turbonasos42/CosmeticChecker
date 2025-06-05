using CosmeticChecker.API.DTOs;
using CosmeticChecker.API.Services;
using DatabaseContext;
using DatabaseModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Api.Controllers;

[Route("api/products/{productId}/reviews")]
[ApiController]
public class ProductReviewsController : ControllerBase
{
    private readonly IProductReviewService _productReviewService;
    private readonly ILogger<ProductReviewsController> _logger;

    public ProductReviewsController(
        IProductReviewService productReviewService,
        ILogger<ProductReviewsController> logger)
    {
        _productReviewService = productReviewService;
        _logger = logger;
    }

    // Получение всех отзывов для продукта с пагинацией
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReviewDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // Получаем отзывы для продукта
        var reviews = await _productReviewService.GetProductReviewsAsync(productId, page, pageSize);

        // Проверяем, есть ли отзывы
        if (!reviews.Any())  // Используем Any для проверки, пустой ли список
        {
            return NotFound("Отзывы не найдены");
        }

        return Ok(reviews);
    }

    // Добавить новый отзыв
    [HttpPost]
    [ProducesResponseType(typeof(ReviewDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ReviewDto>> AddProductReview(int productId, [FromBody] CreateReviewRequest request)
    {
        if (!User.Identity.IsAuthenticated)
            return Unauthorized("Требуется авторизация");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var result = await _productReviewService.AddProductReviewAsync(userId, productId, request);

        if (result != null)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetProductReviews), new { productId }, null);
    }

    // Удалить свой отзыв
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUserReview(int productId)
    {
        if (!User.Identity.IsAuthenticated)
            return Unauthorized("Требуется авторизация");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var result = await _productReviewService.DeleteUserReviewAsync(userId, productId);

        if (result != null)
            return BadRequest(result);

        return NoContent();
    }
}




