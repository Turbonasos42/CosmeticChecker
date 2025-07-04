using CosmeticChecker.API.DTOs;
using Repositories;
using System.Security.Claims;
using static CosmeticChecker.API.Controllers.ReviewModerationController;

namespace CosmeticChecker.API.Services
{
    public class ReviewModerationService : IReviewModerationService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ProductRatingService _productRatingService;
        private readonly ILogger<ReviewModerationService> _logger;

        public ReviewModerationService(IReviewRepository reviewRepository, ILogger<ReviewModerationService> logger, ProductRatingService productRatingService)
        {
            _reviewRepository = reviewRepository;
            _logger = logger;
            _productRatingService = productRatingService;
        }

        public async Task<List<PendingReviewDto>> GetPendingReviewsAsync(int page, int pageSize)
        {
            var reviews = await _reviewRepository.GetPendingReviewsAsync(page, pageSize);
            return reviews.Select(r => new PendingReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                UserId = r.UserId,
                UserName = $"{r.User.FirstName} {r.User.LastName}",
                Rating = r.Rating,
                Text = r.Text,
                CreatedDate = r.CreationDate
            }).ToList();
        }

        public async Task<List<ApprovedReviewDto>> GetApprovedReviewsAsync(int page, int pageSize)
        {
            var reviews = await _reviewRepository.GetApprovedReviewsAsync(page, pageSize);
            return reviews.Select(r => new ApprovedReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                UserId = r.UserId,
                UserName = $"{r.User.FirstName} {r.User.LastName}",
                Rating = r.Rating,
                Text = r.Text,
                CreatedDate = r.CreationDate,
                ModeratedDate = r.ModeratedDate,
                ModeratedBy = r.ModeratedByUser != null ? $"{r.ModeratedByUser.FirstName} {r.ModeratedByUser.LastName}" : "System"
            }).ToList();
        }

        public async Task<List<RejectedReviewDto>> GetRejectedReviewsAsync(int page, int pageSize)
        {
            var reviews = await _reviewRepository.GetRejectedReviewsAsync(page, pageSize);
            return reviews.Select(r => new RejectedReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                UserId = r.UserId,
                UserName = $"{r.User.FirstName} {r.User.LastName}",
                Rating = r.Rating,
                Text = r.Text,
                CreatedDate = r.CreationDate,
                RejectionReason = r.RejectionReason,
                ModeratedDate = r.ModeratedDate,
                ModeratedBy = r.ModeratedByUser != null ? $"{r.ModeratedByUser.FirstName} {r.ModeratedByUser.LastName}" : "System"
            }).ToList();
        }

        // Обновление статуса отзыва (одобрить/отклонить)
        public async Task<string> UpdateReviewStatusAsync(int reviewId, bool isApproved, string rejectionReason, int moderatedById)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (review == null)
                return "Отзыв не найден";

            review.IsApproved = isApproved;
            review.RejectionReason = isApproved ? null : rejectionReason;
            review.ModeratedDate = DateTime.UtcNow;
            review.ModeratedBy = moderatedById;  // Используем переданный ID модератора

            // Сохраняем изменения в репозитории
            await _reviewRepository.UpdateReviewAsync(review);

            // Пересчитываем рейтинг продукта
            await _productRatingService.RecalculateAndSetUserRatingForProductAsync(review.ProductId);

            _logger.LogInformation($"Review {reviewId} status updated by user {moderatedById}. Status: {(isApproved ? "Approved" : "Rejected")}");
            return null;
        }


        public async Task<string> DeleteReviewAsync(int reviewId)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (review == null)
                return "Отзыв не найден";

            await _reviewRepository.RemoveReviewAsync(review);
            await _productRatingService.RecalculateAndSetUserRatingForProductAsync(review.ProductId);

            return null;
        }
    }


}
