using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;

namespace PestControl.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingRepository _repository;

        public BookingsController(IBookingRepository repository)
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
            var booking = _repository.GetById(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Booking booking)
        {
            if (booking.CustomerId <= 0)
                return BadRequest("Valid customer ID is required.");

            _repository.Add(booking);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Booking booking)
        {
            if (_repository.GetById(id) == null) return NotFound();
            booking.Id = id;
            _repository.Update(booking);
            return Ok(booking);
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
