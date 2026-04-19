using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;

namespace PestControl.Api.Controllers
{
    // Tell ASP.NET this handles HTTP requests and validates data automatically
    // The route becomes /api/customers based on the controller name
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        // We keep a reference to the repository interface, not the concrete class
        // This way we can swap implementations without changing anything here
        private readonly ICustomerRepository _repository;

        // .NET automatically gives us the repository — we don't create it ourselves
        public CustomersController(ICustomerRepository repository)
        {
            _repository = repository;
        }

        // Get all customers
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_repository.GetAll()); // 200 OK with list
        }

        // Get a specific customer by ID
        // Returns 404 if they don't exist
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var customer = _repository.GetById(id);
            if (customer == null) return NotFound(); // 404
            return Ok(customer);                     // 200 OK
        }

        // Create a new customer from JSON data in request body
        // Name is required — returns 400 if it's missing
        // Returns 201 Created with the new customer (with database-generated ID)
        [HttpPost]
        public IActionResult Create([FromBody] Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Name))
                return BadRequest("Customer name is required."); // 400

            _repository.Add(customer); // Saves to database, ID gets auto-generated
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer); // 201
        }

        // Update an existing customer
        // Returns 404 if customer doesn't exist
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Customer customer)
        {
            if (_repository.GetById(id) == null) return NotFound(); // 404
            customer.Id = id;
            _repository.Update(customer); // Updates in database
            return Ok(customer);          // 200 OK
        }

        // Delete a customer
        // Returns 404 if they don't exist, 204 No Content on success
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (_repository.GetById(id) == null) return NotFound(); // 404
            _repository.Delete(id); // Removes from database
            return NoContent();     // 204
        }
    }
}