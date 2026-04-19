using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Services;

namespace PestControl.Api.Controllers
{
    // This controller handles all search requests across the system
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        // The search service that does the actual searching
        private readonly SearchService _searchService;

        // .NET automatically provides the search service when creating this controller
        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        // Search endpoint — takes a query string and returns matching results
        // If no query is provided, we reject the request
        [HttpGet]
        public IActionResult Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Search query is required.");

            var results = _searchService.Search(q);
            return Ok(results);
        }
    }
}
