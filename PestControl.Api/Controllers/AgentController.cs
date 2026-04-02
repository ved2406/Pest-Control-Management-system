using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Services;

namespace PestControl.Api.Controllers
{
    // This controller exposes the AI agent to the frontend via two endpoints:
    //   POST /api/agent       — send a message, get an AI response
    //   GET  /api/agent/arms  — see what the agent can do
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        // The PestControlAgent service is injected via DI (registered in Program.cs)
        private readonly PestControlAgent _agent;

        public AgentController(PestControlAgent agent)
        {
            _agent = agent;
        }

        /// <summary>
        /// POST /api/agent
        /// Called by the chat widget (in app.js) every time the user sends a message.
        /// The request body must contain: { "message": "your question here" }
        ///
        /// Flow:
        ///   1. Receives the message from the frontend
        ///   2. Passes it to PestControlAgent.ProcessAsync()
        ///   3. The agent scores all 7 arms, picks the best, fetches SQL data, calls Claude
        ///   4. Returns { arm: "CustomerSearch", message: "Here are the customers..." }
        ///
        /// async/await is used because calling Claude API takes time (network request).
        /// Without async, the thread would be blocked while waiting for Claude to respond.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] AgentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message is required."); // HTTP 400

            var response = await _agent.ProcessAsync(request.Message);
            return Ok(response); // HTTP 200 with { arm, message }
        }

        /// <summary>
        /// GET /api/agent/arms
        /// Returns a list of all 7 arms with their names, descriptions, and trigger keywords.
        /// Useful for seeing exactly what the agent can respond to.
        /// Example response: [{ name: "CustomerSearch", description: "...", triggerKeywords: [...] }]
        /// </summary>
        [HttpGet("arms")]
        public IActionResult GetArms()
        {
            var arms = _agent.GetArms();
            // Project each arm to an anonymous object (only expose what the frontend needs)
            var result = arms.Select(a => new { a.Name, a.Description, a.TriggerKeywords });
            return Ok(result);
        }
    }

    // The request body model for POST /api/agent
    // [FromBody] in the action method will deserialise the JSON body into this class
    public class AgentRequest
    {
        public string Message { get; set; }
    }
}