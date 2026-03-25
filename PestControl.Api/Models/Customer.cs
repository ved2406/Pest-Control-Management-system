namespace PestControl.Api.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PropertyType { get; set; }

        public Customer(int id, string name, string address, string phone, string email, string propertyType)
        {
            Id = id;
            Name = name;
            Address = address;
            Phone = phone;
            Email = email;
            PropertyType = propertyType;
        }
    }
}
