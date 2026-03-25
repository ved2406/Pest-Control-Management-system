namespace PestControl.Api.Models
{
    public class Technician
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specialisation { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool Available { get; set; }

        public Technician(int id, string name, string specialisation, string phone, string email, bool available)
        {
            Id = id;
            Name = name;
            Specialisation = specialisation;
            Phone = phone;
            Email = email;
            Available = available;
        }
    }
}
