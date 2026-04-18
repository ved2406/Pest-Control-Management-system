using PestControl.Api.Models;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Interfaces
{
    /// <summary>
    /// Defines what customer data operations we can do — think of it like a contract.
    /// We're not saying HOW to do them, just WHAT we should be able to do.
    /// Different implementations handle this differently: one talks to SQL Server, another uses fake data for testing.
    /// The rest of the app just uses this interface and doesn't care which one it is.
    /// </summary>
    public interface ICustomerRepository
    {
        // Get all customers from storage
        List<Customer> GetAll();

        // Find a specific customer by their ID — returns null if they don't exist
        Customer GetById(int id);

        // Add a new customer to storage
        void Add(Customer customer);

        // Update a customer's info
        void Update(Customer customer);

        // Remove a customer from storage
        void Delete(int id);

        // Search through customers by name, address, phone, email, or whatever
        List<Customer> Search(string query);
    }
}