using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;

namespace PestControl.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechniciansController : ControllerBase
    {
        private readonly ITechnicianRepository _repository;

        public TechniciansController(ITechnicianRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_repository.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var tech = _repository.GetById(id);
            if (tech == null) return NotFound();
            return Ok(tech);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Technician technician)
        {
            if (string.IsNullOrWhiteSpace(technician.Name))
                return BadRequest("Technician name is required.");

            _repository.Add(technician);
            return CreatedAtAction(nameof(GetById), new { id = technician.Id }, technician);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Technician technician)
        {
            if (_repository.GetById(id) == null) return NotFound();
            technician.Id = id;
            _repository.Update(technician);
            return Ok(technician);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (_repository.GetById(id) == null) return NotFound();
            _repository.Delete(id);
            return NoContent();
        }
    }
}
