namespace PestControl.Api.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int PestTypeId { get; set; }
        public int TechnicianId { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
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
