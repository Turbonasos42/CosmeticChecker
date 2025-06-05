using CosmeticChecker.API.DTOs;
using DatabaseModels;
using Repositories;

namespace CosmeticChecker.API.Services
{
    public class FavoritesService : IFavoritesService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<FavoritesService> _logger;

        public FavoritesService(
            IFavoriteRepository favoriteRepository,
            IProductRepository productRepository,
            ILogger<FavoritesService> logger)
        {
            _favoriteRepository = favoriteRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        // Получить избранные продукты с пагинацией
        public async Task<List<FavoriteProductDto>> GetUserFavoritesAsync(int userId, int page, int pageSize)
        {
            var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId, page, pageSize);
            var totalFavorites = await _favoriteRepository.GetTotalFavoritesCountAsync(userId);

            var favoriteProducts = favorites.Select(f => new FavoriteProductDto
            {
                Id = f.Product.Id,
                Name = f.Product.Name,
                Brand = f.Product.Brand,
                Price = f.Product.Price,
                ProductLink = $"https://example.com/products/{f.Product.Id}"
            }).ToList();

            return favoriteProducts;
        }

        // Добавить продукт в избранное
        public async Task<string> AddToFavoritesAsync(int userId, int productId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null)
                return "Продукт не найден";

            var existingFavorite = await _favoriteRepository.GetFavoriteAsync(userId, productId);
            if (existingFavorite != null)
                return "Продукт уже в вашем избранном";

            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId
            };

            await _favoriteRepository.AddFavoriteAsync(favorite);
            return null;
        }

        // Удалить продукт из избранного
        public async Task<string> RemoveFromFavoritesAsync(int userId, int productId)
        {
            var favorite = await _favoriteRepository.GetFavoriteAsync(userId, productId);
            if (favorite == null)
                return "Продукт не найден в избранном";

            await _favoriteRepository.RemoveFavoriteAsync(favorite);
            _logger.LogInformation($"User {userId} removed product {productId} from favorites");
            return null;
        }
    }

}
