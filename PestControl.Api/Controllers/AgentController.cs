using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Services;

namespace PestControl.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly PestControlAgent _agent;

        public AgentController(PestControlAgent agent)
        {
            _agent = agent;
        }

        /// <summary>
        /// POST /api/agent — Send a message to the AI agent.
        /// The agent selects the best arm, gathers data, and calls Claude API.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] AgentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message is required.");

            var response = await _agent.ProcessAsync(request.Message);
            return Ok(response);
        }

        /// <summary>
        /// GET /api/agent/arms — Returns the list of available agent arms/capabilities.
        /// </summary>
        [HttpGet("arms")]
        public IActionResult GetArms()
        {
            var arms = _agent.GetArms();
            var result = arms.Select(a => new { a.Name, a.Description, a.TriggerKeywords });
            return Ok(result);
        }
    }

    public class AgentRequest
    {
        public string Message { get; set; }
    }
}
