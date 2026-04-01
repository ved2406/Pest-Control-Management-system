namespace PestControl.Api.Services
{
    /// <summary>
    /// Represents a single "arm" (capability/tool) of the AI Agent.
    /// Each arm has a name, description, and a function that executes the action.
    /// The agent selects which arm to use based on the user's intent.
    /// </summary>
    public class AgentArm
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] TriggerKeywords { get; set; }
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
