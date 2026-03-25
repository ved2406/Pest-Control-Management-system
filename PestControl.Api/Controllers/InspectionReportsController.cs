using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;

namespace PestControl.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionReportsController : ControllerBase
    {
        private readonly IInspectionReportRepository _repository;

        public InspectionReportsController(IInspectionReportRepository repository)
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
            var report = _repository.GetById(id);
            if (report == null) return NotFound();
            return Ok(report);
        }

        [HttpGet("booking/{bookingId}")]
        public IActionResult GetByBookingId(int bookingId)
        {
            var report = _repository.GetByBookingId(bookingId);
            if (report == null) return NotFound();
            return Ok(report);
        }

        [HttpPost]
        public IActionResult Create([FromBody] InspectionReport report)
        {
            if (report.BookingId <= 0)
                return BadRequest("Valid booking ID is required.");

            _repository.Add(report);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] InspectionReport report)
        {
            if (_repository.GetById(id) == null) return NotFound();
            report.Id = id;
            _repository.Update(report);
            return Ok(report);
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
