using PestControl.Api.Models;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Interfaces
{
    public interface IPestTypeRepository
    {
        List<PestType> GetAll();
        PestType GetById(int id);
        void Add(PestType pestType);
        void Update(PestType pestType);
        void Delete(int id);
        List<PestType> Search(string query);
    }
}
