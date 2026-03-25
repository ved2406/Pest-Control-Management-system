using Microsoft.AspNetCore.Mvc;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Linq;

namespace PestControl.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ICustomerRepository _customers;
        private readonly IBookingRepository _bookings;
        private readonly ITechnicianRepository _technicians;
        private readonly ITreatmentRepository _treatments;
        private readonly IInspectionReportRepository _reports;

        public DashboardController(
            ICustomerRepository customers,
            IBookingRepository bookings,
            ITechnicianRepository technicians,
            ITreatmentRepository treatments,
            IInspectionReportRepository reports)
        {
            _customers = customers;
            _bookings = bookings;
            _technicians = technicians;
            _treatments = treatments;
            _reports = reports;
        }

        [HttpGet]
        public IActionResult GetStats()
        {
            var allBookings = _bookings.GetAll();
            var allTechnicians = _technicians.GetAll();
            var allReports = _reports.GetAll();

            var stats = new DashboardStats
            {
                TotalCustomers = _customers.GetAll().Count,
                TotalBookings = allBookings.Count,
                ActiveBookings = allBookings.Count(b => b.Status == "Confirmed" || b.Status == "In Progress"),
                CompletedBookings = allBookings.Count(b => b.Status == "Completed"),
                AvailableTechnicians = allTechnicians.Count(t => t.Available),
                TotalTechnicians = allTechnicians.Count,
                PendingFollowUps = allReports.Count(r => r.FollowUpNeeded),
                TotalTreatments = _treatments.GetAll().Count
            };

            return Ok(stats);
        }
    }
}
