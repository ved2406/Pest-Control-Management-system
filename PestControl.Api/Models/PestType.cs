namespace PestControl.Api.Models
{
    public class PestType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
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
