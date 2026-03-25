using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Services;

namespace PestControl.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

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
