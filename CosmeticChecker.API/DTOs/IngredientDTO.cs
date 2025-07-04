namespace CosmeticChecker.API.DTOs;

public class IngredientDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int SafetyLevel { get; set; }
}
public class IngredientMatchDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int SafetyLevel { get; set; }
}

public class CompositionSearchResultDto
{
    public double ProductRating { get; set; }  // Общий рейтинг продукта
    public List<IngredientMatchDto> MatchedIngredients { get; set; } // Список ингредиентов с совпадениями
}