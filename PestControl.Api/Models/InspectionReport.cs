namespace PestControl.Api.Models
{
    public class InspectionReport
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string Findings { get; set; }
        public string Recommendations { get; set; }
        public bool FollowUpNeeded { get; set; }
        public string ReportDate { get; set; }

        public InspectionReport(int id, int bookingId, string findings, string recommendations,
            bool followUpNeeded, string reportDate)
        {
            Id = id;
            BookingId = bookingId;
            Findings = findings;
            Recommendations = recommendations;
            FollowUpNeeded = followUpNeeded;
            ReportDate = reportDate;
        }
    }
}
