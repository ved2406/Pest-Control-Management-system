namespace PestControl.Api.Services
{
    // Represents one capability of the AI agent
    // The agent scores all arms against the user message using keyword matching and invokes the best match
    public class AgentArm
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] TriggerKeywords { get; set; }

        // Fetches relevant database data to pass as context to Claude
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
