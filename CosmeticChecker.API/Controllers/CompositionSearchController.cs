using CosmeticChecker.API.DTOs;
using CosmeticChecker.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CosmeticChecker.API.Controllers
{
    [Route("api/ingredients")]
    [ApiController]
    public class CompositionSearchController : ControllerBase
    {
        private readonly ICompositionSearchService _compositionSearchService;

        public CompositionSearchController (ICompositionSearchService compositionSearchService)
        {
            _compositionSearchService = compositionSearchService;
        }

        // Получение списка ингредиентов по составу
        [HttpPost("search")]
        public async Task<ActionResult<CompositionSearchResultDto>> SearchComposition([FromBody] string composition)
        {
            var result = await _compositionSearchService.SearchCompositionAsync(composition);
            return Ok(result);
        }
    }
}
