namespace PestControl.Api.Models
{
    public class Treatment
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Method { get; set; }
        public int TargetPestTypeId { get; set; }
        public string SafetyInfo { get; set; }

        public Treatment(int id, string productName, string method, int targetPestTypeId, string safetyInfo)
        {
            Id = id;
            ProductName = productName;
            Method = method;
            TargetPestTypeId = targetPestTypeId;
            SafetyInfo = safetyInfo;
        }
    }
}
