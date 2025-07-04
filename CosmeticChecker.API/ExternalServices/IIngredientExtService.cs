using System.Threading.Tasks;

namespace CosmeticChecker.API.ExternalServices
{
    public interface IIngredientExtService
    {
        // Метод для добавления ингредиентов из JSON файла в базу данных
        Task AddIngredientsFromJsonAsync(string jsonFilePath);
        Task LinkAllIngredientsToProductsAsync();
        Task AddIngredientToProductAsync(int productId, int ingredientId);
    }
}
