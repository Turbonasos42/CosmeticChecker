using CosmeticChecker.API.DTOs;
using DatabaseModels;
using Repositories;

namespace CosmeticChecker.API.Services
{
    public class ProductReviewService : IProductReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;
        private readonly ProductRatingService _productRatingService;
        private readonly ILogger<ProductReviewService> _logger;
        

        public ProductReviewService(
            IReviewRepository reviewRepository,
            IProductRepository productRepository,
            ProductRatingService productRatingService,
            ILogger<ProductReviewService> logger)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _productRatingService = productRatingService;
            _logger = logger;
        }

        // Получить все отзывы для продукта с пагинацией
        public async Task<List<ReviewDto>> GetProductReviewsAsync(int productId, int page, int pageSize)
        {
            var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId, page, pageSize);

            return reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                ProductId = r.ProductId,
                Text = r.Text,
                Rating = r.Rating,
                CreationDate = r.CreationDate,
                UserName = $"{r.User.FirstName} {r.User.LastName}"
            }).ToList();
        }

        // Добавить отзыв для продукта
        public async Task<string> AddProductReviewAsync(int userId, int productId, CreateReviewRequest request)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
                return "Продукт не найден";

            var existingReview = await _reviewRepository.GetReviewByUserAsync(userId, productId);
            if (existingReview != null)
                return "Вы уже оставляли отзыв для этого продукта";

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = request.Rating,
                Text = request.Text,
                CreationDate = DateTime.UtcNow,
                IsApproved = false // по умолчанию не одобрен
            };

            await _reviewRepository.AddReviewAsync(review);
            await _productRatingService.RecalculateAndSetUserRatingForProductAsync(productId);
            return null;
        }

        // Удалить свой отзыв для продукта
        public async Task<string> DeleteUserReviewAsync(int userId, int productId)
        {
            var review = await _reviewRepository.GetReviewByUserAsync(userId, productId);
            if (review == null)
                return "Отзыв не найден или уже удален";

            await _reviewRepository.RemoveReviewAsync(review);
            _logger.LogInformation($"User {userId} deleted their review for product {productId}");
            await _productRatingService.RecalculateAndSetUserRatingForProductAsync(productId);
            return null;
        }
    }

}
