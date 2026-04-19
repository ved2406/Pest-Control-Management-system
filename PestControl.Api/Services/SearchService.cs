using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Services
{
    // This service is like a universal search — it looks through all the different data stores
    // (customers, pest types, bookings, technicians, treatments, reports) and collects all the matches
    // Then it gives you back one combined list of results so you don't have to search each thing separately
    public class SearchService
    {
        // We keep references to all the repositories — basically connections to all the different data sources
        private readonly ICustomerRepository _customers;
        private readonly IPestTypeRepository _pestTypes;
        private readonly IBookingRepository _bookings;
        private readonly ITechnicianRepository _technicians;
        private readonly ITreatmentRepository _treatments;
        private readonly IInspectionReportRepository _reports;

        // .NET automatically provides all the repositories we need when creating this service
        // This is dependency injection — we don't create them ourselves, they just get passed in
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

        // Takes a search query and looks through everything, returning matches in a combined list
        // If the query is empty or just whitespace, we return an empty list
        public List<SearchResult> Search(string query)
        {
            var results = new List<SearchResult>();

            if (string.IsNullOrWhiteSpace(query))
                return results;

            // Search customers and format their results with address and property type info
            foreach (var c in _customers.Search(query))
            {
                results.Add(new SearchResult("Customer", c.Id, c.Name, c.Address + " | " + c.PropertyType));
            }

            // Search pest types and add their category and risk level to the results
            foreach (var p in _pestTypes.Search(query))
            {
                results.Add(new SearchResult("Pest Type", p.Id, p.Name, p.Category + " | Risk: " + p.RiskLevel));
            }

            // Search through bookings and include their status and location
            foreach (var b in _bookings.Search(query))
            {
                results.Add(new SearchResult("Booking", b.Id, "Booking #" + b.Id + " - " + b.Date, b.Status + " | " + b.Location));
            }

            // Search technicians and show their specialty and availability status
            foreach (var t in _technicians.Search(query))
            {
                results.Add(new SearchResult("Technician", t.Id, t.Name, t.Specialisation + " | " + (t.Available ? "Available" : "Unavailable")));
            }

            // Search treatments to find matching products and methods
            foreach (var t in _treatments.Search(query))
            {
                results.Add(new SearchResult("Treatment", t.Id, t.ProductName, t.Method));
            }

            // Search inspection reports and truncate the findings if they're too long (show first 80 chars with ...)
            foreach (var r in _reports.Search(query))
            {
                results.Add(new SearchResult("Inspection Report", r.Id, "Report #" + r.Id + " - " + r.ReportDate, r.Findings.Length > 80 ? r.Findings.Substring(0, 80) + "..." : r.Findings));
            }

            return results;
        }
    }
}
