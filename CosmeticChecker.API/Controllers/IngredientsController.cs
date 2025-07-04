using CosmeticChecker.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using CosmeticChecker.API.Services;

namespace CosmeticChecker.API.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/ingredients")]
    public class IngredientsController : ControllerBase
    {
        private readonly IIngredientService _ingredientService;

        public IngredientsController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        [HttpGet]
        public async Task<ActionResult<List<IngredientDto>>> GetIngredientsByProductId(int productId)
        {
            var result = await _ingredientService.GetIngredientsByProductIdAsync(productId);
            return Ok(result);
        }

       
    }

    
}
