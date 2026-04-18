namespace PestControl.Api.Models
{
    /// <summary>
    /// A scheduled pest control appointment. We store references to the customer, pest type, 
    /// and technician rather than their names to keep things DRY and avoid duplicating data.
    /// </summary>
    public class Booking
    {
        // Unique identifier for this booking
        public int Id { get; set; }

        // Which customer booked this appointment
        public int CustomerId { get; set; }

        // What type of pest we're dealing with
        public int PestTypeId { get; set; }

        // Which technician is assigned to handle this
        public int TechnicianId { get; set; }

        // When the appointment is scheduled
        public string Date { get; set; }

        // What time they're coming
        public string Time { get; set; }

        // Where things are at: "Pending", "Confirmed", "In Progress", or "Completed"
        public string Status { get; set; }

        // Where the job needs to happen
        public string Location { get; set; }

        // Any special details or instructions
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