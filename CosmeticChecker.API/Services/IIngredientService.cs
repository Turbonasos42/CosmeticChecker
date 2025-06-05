using CosmeticChecker.API.DTOs;

namespace CosmeticChecker.API.Services
{
    public interface IIngredientService
    {
        Task<List<IngredientDto>> GetIngredientsByProductIdAsync(int productId);
    }
}
