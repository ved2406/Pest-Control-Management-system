using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;

namespace PestControl.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerRepository _repository;

        public CustomersController(ICustomerRepository repository)
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
            var customer = _repository.GetById(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Name))
                return BadRequest("Customer name is required.");

            _repository.Add(customer);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Customer customer)
        {
            if (_repository.GetById(id) == null) return NotFound();
            customer.Id = id;
            _repository.Update(customer);
            return Ok(customer);
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
