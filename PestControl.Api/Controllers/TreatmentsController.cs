using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;

namespace PestControl.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TreatmentsController : ControllerBase
    {
        private readonly ITreatmentRepository _repository;

        public TreatmentsController(ITreatmentRepository repository)
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
            var treatment = _repository.GetById(id);
            if (treatment == null) return NotFound();
            return Ok(treatment);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Treatment treatment)
        {
            if (string.IsNullOrWhiteSpace(treatment.ProductName))
                return BadRequest("Product name is required.");

            _repository.Add(treatment);
            return CreatedAtAction(nameof(GetById), new { id = treatment.Id }, treatment);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Treatment treatment)
        {
            if (_repository.GetById(id) == null) return NotFound();
            treatment.Id = id;
            _repository.Update(treatment);
            return Ok(treatment);
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
