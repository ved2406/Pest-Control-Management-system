using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Services
{
    public class SearchService
    {
        private readonly ICustomerRepository _customers;
        private readonly IPestTypeRepository _pestTypes;
        private readonly IBookingRepository _bookings;
        private readonly ITechnicianRepository _technicians;
        private readonly ITreatmentRepository _treatments;
        private readonly IInspectionReportRepository _reports;

        public SearchService(
            ICustomerRepository customers,
            IPestTypeRepository pestTypes,
            IBookingRepository bookings,
            ITechnicianRepository technicians,
            ITreatmentRepository treatments,
            IInspectionReportRepository reports)
        {
            _customers = customers;
            _pestTypes = pestTypes;
            _bookings = bookings;
            _technicians = technicians;
            _treatments = treatments;
            _reports = reports;
        }

        public List<SearchResult> Search(string query)
        {
            var results = new List<SearchResult>();

            if (string.IsNullOrWhiteSpace(query))
                return results;

            foreach (var c in _customers.Search(query))
            {
                results.Add(new SearchResult("Customer", c.Id, c.Name, c.Address + " | " + c.PropertyType));
            }

            foreach (var p in _pestTypes.Search(query))
            {
                results.Add(new SearchResult("Pest Type", p.Id, p.Name, p.Category + " | Risk: " + p.RiskLevel));
            }

            foreach (var b in _bookings.Search(query))
            {
                results.Add(new SearchResult("Booking", b.Id, "Booking #" + b.Id + " - " + b.Date, b.Status + " | " + b.Location));
            }

            foreach (var t in _technicians.Search(query))
            {
                results.Add(new SearchResult("Technician", t.Id, t.Name, t.Specialisation + " | " + (t.Available ? "Available" : "Unavailable")));
            }

            foreach (var t in _treatments.Search(query))
            {
                results.Add(new SearchResult("Treatment", t.Id, t.ProductName, t.Method));
            }

            foreach (var r in _reports.Search(query))
            {
                results.Add(new SearchResult("Inspection Report", r.Id, "Report #" + r.Id + " - " + r.ReportDate, r.Findings.Length > 80 ? r.Findings.Substring(0, 80) + "..." : r.Findings));
            }

            return results;
        }
    }
}
