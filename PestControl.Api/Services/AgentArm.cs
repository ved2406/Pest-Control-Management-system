namespace PestControl.Api.Services
{
    /// <summary>
    /// AgentArm represents one capability (or "tool") that the AI agent has.
    /// Think of it like a specialist — each arm knows about one area of the system.
    ///
    /// The agent has 7 arms:
    ///   CustomerSearch, TechnicianAvailability, TreatmentRecommendation,
    ///   BookingLookup, PestInfo, ReportSummary, DashboardStats
    ///
    /// When the user sends a message, the agent scores all arms using keyword matching
    /// and picks the arm with the highest score to handle the query.
    /// </summary>
    public class AgentArm
    {
        // Unique name for this arm — used to identify which arm handled the request
        // e.g. "CustomerSearch", "PestInfo"
        public string Name { get; set; }

        // Human-readable description of what this arm does
        // e.g. "Search for customers by name, email, phone or address"
        public string Description { get; set; }

        // Words or phrases that trigger this arm.
        // If the user's message contains any of these, this arm gets score points.
        // Multi-word keywords (e.g. "find customer") score higher than single words.
        public string[] TriggerKeywords { get; set; }

        // A function (delegate) that takes the user's message as input
        // and returns a formatted string of data fetched from the SQL database.
        // This string is sent to Claude as context so it can give accurate answers.
        // Func<string, string> means: takes a string in, returns a string out.
        public Func<string, string> Execute { get; set; }

        public AgentArm(string name, string description, string[] triggerKeywords, Func<string, string> execute)
        {
            Name = name;
            Description = description;
            TriggerKeywords = triggerKeywords;
            Execute = execute;
        }
    }
}