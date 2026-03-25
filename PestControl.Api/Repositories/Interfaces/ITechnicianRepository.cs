using PestControl.Api.Models;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Interfaces
{
    public interface ITechnicianRepository
    {
        List<Technician> GetAll();
        Technician GetById(int id);
        void Add(Technician technician);
        void Update(Technician technician);
        void Delete(int id);
        List<Technician> Search(string query);
    }
}
