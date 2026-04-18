using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;

namespace PestControl.Api.Controllers
{
    // This handles all the API endpoints for bookings — basically the interface between the client and the booking data
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        // We use the repository to talk to the database or test data store
        private readonly IBookingRepository _repository;

        // .NET automatically gives us the repository when this controller is created
        public BookingsController(IBookingRepository repository)
        {
            _repository = repository;
        }

        // Get the list of all bookings
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_repository.GetAll());
        }

        // Look up a specific booking using its ID — returns nothing if it doesn't exist
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var booking = _repository.GetById(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        // Create a new booking from the data sent in the request — customer ID is required and must be valid
        [HttpPost]
        public IActionResult Create([FromBody] Booking booking)
        {
            if (booking.CustomerId <= 0)
                return BadRequest("Valid customer ID is required.");

            _repository.Add(booking);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }

        // Update the details of an existing booking — if the booking doesn't exist, nothing happens
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Booking booking)
        {
            if (_repository.GetById(id) == null) return NotFound();
            booking.Id = id;
            _repository.Update(booking);
            return Ok(booking);
        }

        // Remove a booking from the system — if it doesn't exist, you'll get a 404
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (_repository.GetById(id) == null) return NotFound();
            _repository.Delete(id);
            return NoContent();
        }
    }
}
