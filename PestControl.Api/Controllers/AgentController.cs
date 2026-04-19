using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Services;

namespace PestControl.Api.Controllers
{
    // Exposes the AI agent via two endpoints
    // POST /api/agent      - send a message get an AI response
    // GET  /api/agent/arms - list all agent capabilities
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly PestControlAgent _agent;

        public AgentController(PestControlAgent agent)
        {
            _agent = agent;
        }

        // Accepts { "message": "..." } and returns { arm message } from Claude
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] AgentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message is required.");

            var response = await _agent.ProcessAsync(request.Message);
            return Ok(response);
        }

        // Returns all registered arms with their names descriptions and trigger keywords
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
