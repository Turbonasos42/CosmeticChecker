using CosmeticChecker.API.DTOs;

namespace CosmeticChecker.API.Services
{
    public interface IFavoritesService
    {
        Task<List<FavoriteProductDto>> GetUserFavoritesAsync(int userId, int page, int pageSize);
        Task<string> AddToFavoritesAsync(int userId, int productId);
        Task<string> RemoveFromFavoritesAsync(int userId, int productId);
    }

}
