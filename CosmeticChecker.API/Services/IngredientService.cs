using CosmeticChecker.API.DTOs;
using Repositories;

namespace CosmeticChecker.API.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly IIngredientRepository _ingredientRepository;

        public IngredientService(IIngredientRepository ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }

        public async Task<List<IngredientDto>> GetIngredientsByProductIdAsync(int productId)
        {
            var ingredients = await _ingredientRepository.GetIngredientsByProductIdAsync(productId);

            return ingredients.Select(i => new IngredientDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                SafetyLevel = i.SafetyLevel
            }).ToList();
        }

    }
}
