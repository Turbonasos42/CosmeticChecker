using CosmeticChecker.API.DTOs;

namespace CosmeticChecker.API.Services
{
    public interface IProductReviewService
    {
        Task<List<ReviewDto>> GetProductReviewsAsync(int productId, int page, int pageSize);
        Task<string> AddProductReviewAsync(int userId, int productId, CreateReviewRequest request);
        Task<string> DeleteUserReviewAsync(int userId, int productId);
    }

}
