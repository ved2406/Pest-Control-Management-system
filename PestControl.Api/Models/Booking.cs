namespace PestControl.Api.Models
{
    /// <summary>
    /// Booking represents a scheduled pest control appointment.
    /// Maps directly to the "Bookings" table in SQL Server.
    ///
    /// Notice that Booking does NOT store the customer's name or pest type's name directly.
    /// Instead it stores their IDs (CustomerId, PestTypeId, TechnicianId).
    /// These are FOREIGN KEYS — they link to the Id column in the Customers, PestTypes,
    /// and Technicians tables respectively.
    ///
    /// This is called a Relational Database design — data is not duplicated,
    /// it is referenced by ID. When we need names, we "join" or look up using the IDs.
    /// </summary>
    public class Booking
    {
        // Auto-generated primary key
        public int Id { get; set; }

        // Foreign key — links to Customers.Id
        public int CustomerId { get; set; }

        // Foreign key — links to PestTypes.Id
        public int PestTypeId { get; set; }

        // Foreign key — links to Technicians.Id
        public int TechnicianId { get; set; }

        // Date of the appointment e.g. "2025-06-15"
        public string Date { get; set; }

        // Time of the appointment e.g. "09:00"
        public string Time { get; set; }

        // Current status: "Pending", "Confirmed", "In Progress", or "Completed"
        public string Status { get; set; }

        // Address where the job will take place
        public string Location { get; set; }

        // Any extra notes about the job
        public string Notes { get; set; }

        public Booking(int id, int customerId, int pestTypeId, int technicianId,
            string date, string time, string status, string location, string notes)
        {
            Id = id;
            CustomerId = customerId;
            PestTypeId = pestTypeId;
            TechnicianId = technicianId;
            Date = date;
            Time = time;
            Status = status;
            Location = location;
            Notes = notes;
        }
    }
}