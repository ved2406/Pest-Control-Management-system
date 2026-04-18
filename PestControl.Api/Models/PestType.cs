namespace PestControl.Api.Models
{
    /// <summary>
    /// This is basically information about a pest we might need to treat — like an entry 
    /// in a catalog. We store what it's called, what kind of pest it is, some context about 
    /// it, and how serious dealing with it is.
    /// </summary>
    public class PestType
    {
        // Just a number to identify each pest type
        public int Id { get; set; }

        // What we call it (like "Cockroach", "Termite", etc)
        public string Name { get; set; }

        // The general type it falls under (Insect, Rodent, that sort of thing)
        public string Category { get; set; }

        // Some context about the pest — why it matters, what damage it can cause, etc
        public string Description { get; set; }

        // How bad it is if someone has it (Low, Medium, High)
        public string RiskLevel { get; set; }

        public PestType(int id, string name, string category, string description, string riskLevel)
        {
            Id = id;
            Name = name;
            Category = category;
            Description = description;
            RiskLevel = riskLevel;
        }
    }
}
