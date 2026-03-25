using PestControl.Api.Models;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Interfaces
{
    public interface ITreatmentRepository
    {
        List<Treatment> GetAll();
        Treatment GetById(int id);
        void Add(Treatment treatment);
        void Update(Treatment treatment);
        void Delete(int id);
        List<Treatment> Search(string query);
    }
}
