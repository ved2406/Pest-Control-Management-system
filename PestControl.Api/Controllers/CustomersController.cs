using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;

namespace PestControl.Api.Controllers
{
    // [ApiController] tells .NET this class handles HTTP requests and auto-validates request bodies
    // [Route("api/[controller]")] sets the base URL to /api/customers
    // [controller] is replaced with the class name minus "Controller" — so "Customers"
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        // We store the repository as a private field.
        // We use the INTERFACE (ICustomerRepository) not the concrete class —
        // this means we could swap SqlCustomerRepository for StaticCustomerRepository
        // without changing any code here. This is called Dependency Inversion.
        private readonly ICustomerRepository _repository;

        // Constructor Injection — .NET automatically provides the ICustomerRepository
        // that was registered in Program.cs. We never call "new" ourselves.
        public CustomersController(ICustomerRepository repository)
        {
            _repository = repository;
        }

        // GET /api/customers
        // Returns all customers as a JSON array
        // HTTP 200 OK with the list of customers in the response body
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_repository.GetAll()); // Ok() = HTTP 200
        }

        // GET /api/customers/5
        // {id} in the route captures the number from the URL and passes it as the parameter
        // Returns HTTP 404 if no customer with that ID exists
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var customer = _repository.GetById(id);
            if (customer == null) return NotFound(); // HTTP 404
            return Ok(customer);                     // HTTP 200
        }

        // POST /api/customers
        // [FromBody] means .NET reads the JSON from the request body and deserialises it into a Customer object
        // Validates that the name is not empty before inserting
        // Returns HTTP 201 Created with the new customer (including its new SQL-generated ID)
        [HttpPost]
        public IActionResult Create([FromBody] Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Name))
                return BadRequest("Customer name is required."); // HTTP 400

            _repository.Add(customer); // SQL INSERT — customer.Id is updated with the new ID
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer); // HTTP 201
        }

        // PUT /api/customers/5
        // Updates an existing customer. Returns 404 if the customer doesn't exist.
        // Sets customer.Id = id to prevent the URL id and body id from conflicting.
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Customer customer)
        {
            if (_repository.GetById(id) == null) return NotFound(); // HTTP 404
            customer.Id = id;
            _repository.Update(customer); // SQL UPDATE
            return Ok(customer);          // HTTP 200
        }

        // DELETE /api/customers/5
        // Deletes a customer by ID. Returns 204 No Content on success (nothing to return).
        // Returns 404 if the customer doesn't exist.
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (_repository.GetById(id) == null) return NotFound(); // HTTP 404
            _repository.Delete(id); // SQL DELETE
            return NoContent();     // HTTP 204
        }
    }
}