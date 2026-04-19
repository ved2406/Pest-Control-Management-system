namespace PestControl.Api.Models
{
    public class DashboardStats
    {
        public int TotalCustomers { get; set; }
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int AvailableTechnicians { get; set; }
        public int TotalTechnicians { get; set; }
        public int PendingFollowUps { get; set; }
        public int TotalTreatments { get; set; }
    }
}
