using CosmeticChecker.API.DTOs;
using Repositories;

namespace CosmeticChecker.API.Services
{
    public interface ICompositionSearchService
    {
        Task<CompositionSearchResultDto> SearchCompositionAsync(string composition);
    }
}
