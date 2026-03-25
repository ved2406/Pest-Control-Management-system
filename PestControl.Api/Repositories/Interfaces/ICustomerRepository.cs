using PestControl.Api.Models;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        List<Customer> GetAll();
        Customer GetById(int id);
        void Add(Customer customer);
        void Update(Customer customer);
        void Delete(int id);
        List<Customer> Search(string query);
    }
}
