namespace PestControl.Api.Models
{
    /// <summary>
    /// One of our actual technicians — the people who show up at jobs and handle the pest stuff.
    /// We need to know who they are, what they're good at treating, how to contact them, and if they're free.
    /// </summary>
    public class Technician
    {
        // ID to identify them
        public int Id { get; set; }

        // Their name
        public string Name { get; set; }

        // Their specialty — maybe they're really good with termites, or rodents, whatever
        public string Specialisation { get; set; }

        // Phone number to call them
        public string Phone { get; set; }

        // Email to reach them
        public string Email { get; set; }

        // Are they working on something or can they take a new job?
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
