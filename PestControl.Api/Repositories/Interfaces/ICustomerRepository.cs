using PestControl.Api.Models;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Interfaces
{
    /// <summary>
    /// ICustomerRepository defines the CONTRACT for all customer data operations.
    ///
    /// This is the Repository Pattern — we define WHAT operations are available here,
    /// but not HOW they are implemented. There are two implementations:
    ///   - SqlCustomerRepository  : runs real SQL queries against SQL Server (used in production)
    ///   - StaticCustomerRepository : uses in-memory hardcoded data (used in unit tests)
    ///
    /// Controllers and the AI agent only ever talk to this interface.
    /// This means we can swap the underlying data source without changing any other code.
    /// This is called the Dependency Inversion Principle.
    /// </summary>
    public interface ICustomerRepository
    {
        // Get every customer in the database — SQL: SELECT * FROM Customers
        List<Customer> GetAll();

        // Get one customer by their ID — SQL: SELECT * FROM Customers WHERE Id = @Id
        // Returns null if no customer with that ID exists
        Customer GetById(int id);

        // Insert a new customer — SQL: INSERT INTO Customers (...)
        // After calling this, customer.Id will be updated with the new auto-generated ID
        void Add(Customer customer);

        // Update an existing customer's details — SQL: UPDATE Customers SET ... WHERE Id = @Id
        void Update(Customer customer);

        // Remove a customer from the database — SQL: DELETE FROM Customers WHERE Id = @Id
        void Delete(int id);

        // Search customers by name, address, phone, email, or property type
        // Uses SQL LIKE with wildcards: WHERE Name LIKE '%query%'
        List<Customer> Search(string query);
    }
}