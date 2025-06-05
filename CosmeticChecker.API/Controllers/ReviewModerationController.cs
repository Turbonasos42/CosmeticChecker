using CosmeticChecker.API.DTOs;
using CosmeticChecker.API.Services;
using DatabaseContext;
using DatabaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;

namespace CosmeticChecker.API.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    [Route("api/moderate/reviews")]
    [ApiController]
    public class ReviewModerationController : ControllerBase
    {
        private readonly IReviewModerationService _reviewModerationService;
        private readonly ILogger<ReviewModerationController> _logger;

        public ReviewModerationController(
            IReviewModerationService reviewModerationService,
            ILogger<ReviewModerationController> logger)
        {
            _reviewModerationService = reviewModerationService;
            _logger = logger;
        }

        // Получить отзывы, ожидающие модерации
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<PendingReviewDto>>> GetPendingReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var reviews = await _reviewModerationService.GetPendingReviewsAsync(page, pageSize);
            return Ok(reviews);
        }

        // Получить одобренные отзывы
        [HttpGet("approved")]
        public async Task<ActionResult<IEnumerable<ApprovedReviewDto>>> GetApprovedReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var reviews = await _reviewModerationService.GetApprovedReviewsAsync(page, pageSize);
            return Ok(reviews);
        }

        // Получить отклоненные отзывы
        [HttpGet("rejected")]
        public async Task<ActionResult<IEnumerable<RejectedReviewDto>>> GetRejectedReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var reviews = await _reviewModerationService.GetRejectedReviewsAsync(page, pageSize);
            return Ok(reviews);
        }

        // Обновление статуса отзыва (одобрить/отклонить)
        [HttpPatch("{reviewId}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateReviewStatusAsync(int reviewId, [FromBody] UpdateReviewStatusRequest request)
        {
            // Извлекаем ID модератора из контекста
            var moderatedById = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _reviewModerationService.UpdateReviewStatusAsync(reviewId, request.IsApproved, request.RejectionReason, moderatedById);

            if (result != null)
                return BadRequest(new { Message = result });

            return NoContent();
        }
        public class UpdateReviewStatusRequest
        {
            public bool IsApproved { get; set; }
            public string RejectionReason { get; set; }
        }



        // Удалить отзыв
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var result = await _reviewModerationService.DeleteReviewAsync(reviewId);
            if (result != null)
                return BadRequest(result);

            return NoContent();
        }
    }



}
