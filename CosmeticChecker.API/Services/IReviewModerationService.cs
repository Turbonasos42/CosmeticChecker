using CosmeticChecker.API.DTOs;
using static CosmeticChecker.API.Controllers.ReviewModerationController;

namespace CosmeticChecker.API.Services
{
    public interface IReviewModerationService
    {
        Task<List<PendingReviewDto>> GetPendingReviewsAsync(int page, int pageSize);
        Task<List<ApprovedReviewDto>> GetApprovedReviewsAsync(int page, int pageSize);
        Task<List<RejectedReviewDto>> GetRejectedReviewsAsync(int page, int pageSize);
        Task<string> UpdateReviewStatusAsync(int reviewId, bool isApproved, string rejectionReason, int moderatedById); // Изменено
        Task<string> DeleteReviewAsync(int reviewId);
    }

}
