using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;

namespace PestControl.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PestTypesController : ControllerBase
    {
        private readonly IPestTypeRepository _repository;

        public PestTypesController(IPestTypeRepository repository)
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
            var pest = _repository.GetById(id);
            if (pest == null) return NotFound();
            return Ok(pest);
        }

        [HttpPost]
        public IActionResult Create([FromBody] PestType pestType)
        {
            if (string.IsNullOrWhiteSpace(pestType.Name))
                return BadRequest("Pest type name is required.");

            _repository.Add(pestType);
            return CreatedAtAction(nameof(GetById), new { id = pestType.Id }, pestType);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] PestType pestType)
        {
            if (_repository.GetById(id) == null) return NotFound();
            pestType.Id = id;
            _repository.Update(pestType);
            return Ok(pestType);
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
