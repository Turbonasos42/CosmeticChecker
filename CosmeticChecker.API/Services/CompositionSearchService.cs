using CosmeticChecker.API.DTOs;
using Repositories;

namespace CosmeticChecker.API.Services
{
    public class CompositionSearchService : ICompositionSearchService
    {
        private readonly IIngredientRepository _ingredientRepository;

        public CompositionSearchService(IIngredientRepository ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }

        public async Task<CompositionSearchResultDto> SearchCompositionAsync(string composition)
        {
            // Получаем все ингредиенты, которые соответствуют введенному составу
            var ingredients = await _ingredientRepository.SearchIngredientsByNameAsync(composition);

            // Если ингредиенты найдены
            if (ingredients.Any())
            {
                var matchedIngredients = ingredients.Select(i => new IngredientMatchDto
                {
                    Name = i.Name,
                    Description = i.Description,
                    SafetyLevel = i.SafetyLevel
                }).ToList();

                // Расчет общего рейтинга продукта на основе найденных ингредиентов
                var productRating = matchedIngredients.Average(i => i.SafetyLevel);

                return new CompositionSearchResultDto
                {
                    ProductRating = Math.Round(productRating, 2),
                    MatchedIngredients = matchedIngredients
                };
            }

            return new CompositionSearchResultDto
            {
                ProductRating = 0,  // Если ингредиенты не найдены, возвращаем рейтинг 0
                MatchedIngredients = new List<IngredientMatchDto>()
            };
        }
    }
}
