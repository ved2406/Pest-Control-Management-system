using Microsoft.Data.SqlClient;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Sql
{
    /// <summary>
    /// SqlCustomerRepository implements ICustomerRepository using real SQL Server queries.
    /// Uses Microsoft.Data.SqlClient (ADO.NET) to talk directly to the database.
    /// We do NOT use Entity Framework — every query is written manually in SQL.
    ///
    /// Key safety feature: ALL queries use PARAMETERISED inputs (e.g. @Name, @Id).
    /// This prevents SQL Injection — a common attack where malicious SQL is inserted into input fields.
    /// For example, instead of: "WHERE Name = '" + name + "'"  (UNSAFE)
    /// We write:                 "WHERE Name = @Name"  then set @Name separately (SAFE)
    /// </summary>
    public class SqlCustomerRepository : ICustomerRepository
    {
        // The connection string tells ADO.NET how to connect to SQL Server
        private readonly string _connectionString;

        public SqlCustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Gets all customers from the database.
        /// Uses a SqlDataReader to read rows one at a time and map them to Customer objects.
        /// The "using" keyword ensures the connection is automatically closed and disposed
        /// even if an error occurs — prevents connection leaks.
        /// </summary>
        public List<Customer> GetAll()
        {
            var customers = new List<Customer>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, Name, Address, Phone, Email, PropertyType FROM Customers", connection);
            using var reader = command.ExecuteReader(); // runs the query and returns a row-by-row reader
            while (reader.Read()) // move to next row — returns false when no more rows
            {
                customers.Add(MapCustomer(reader)); // convert the current row into a Customer object
            }
            return customers;
        }

        /// <summary>
        /// Gets a single customer by ID.
        /// Uses @Id parameter — prevents SQL injection.
        /// Returns null if no customer with that ID is found.
        /// </summary>
        public Customer GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "SELECT Id, Name, Address, Phone, Email, PropertyType FROM Customers WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id); // safely bind the id value to @Id
            using var reader = command.ExecuteReader();
            if (reader.Read()) // only one row expected
            {
                return MapCustomer(reader);
            }
            return null; // no customer found with this ID
        }

        /// <summary>
        /// Inserts a new customer into the database.
        /// OUTPUT INSERTED.Id tells SQL Server to return the auto-generated ID back to us.
        /// ExecuteScalar() runs the query and returns the single value (the new ID).
        /// We assign it back to customer.Id so the caller can see the new ID.
        /// </summary>
        public void Add(Customer customer)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "INSERT INTO Customers (Name, Address, Phone, Email, PropertyType) " +
                "OUTPUT INSERTED.Id " +   // returns the new auto-generated ID
                "VALUES (@Name, @Address, @Phone, @Email, @PropertyType)", connection);
            // Bind each parameter — this is what prevents SQL injection
            command.Parameters.AddWithValue("@Name", customer.Name);
            command.Parameters.AddWithValue("@Address", customer.Address);
            command.Parameters.AddWithValue("@Phone", customer.Phone);
            command.Parameters.AddWithValue("@Email", customer.Email);
            command.Parameters.AddWithValue("@PropertyType", customer.PropertyType);
            customer.Id = (int)command.ExecuteScalar(); // save the new ID back into the object
        }

        /// <summary>
        /// Updates all fields for an existing customer.
        /// ExecuteNonQuery() runs an UPDATE/DELETE/INSERT that doesn't return rows.
        /// </summary>
        public void Update(Customer customer)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "UPDATE Customers SET Name = @Name, Address = @Address, Phone = @Phone, " +
                "Email = @Email, PropertyType = @PropertyType WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", customer.Id);
            command.Parameters.AddWithValue("@Name", customer.Name);
            command.Parameters.AddWithValue("@Address", customer.Address);
            command.Parameters.AddWithValue("@Phone", customer.Phone);
            command.Parameters.AddWithValue("@Email", customer.Email);
            command.Parameters.AddWithValue("@PropertyType", customer.PropertyType);
            command.ExecuteNonQuery(); // runs the UPDATE, returns number of rows affected
        }

        /// <summary>
        /// Deletes a customer by ID.
        /// </summary>
        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("DELETE FROM Customers WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Searches customers using SQL LIKE with % wildcards on both sides.
        /// % means "any characters" — so "%john%" matches "John Smith", "Johnny", etc.
        /// Searches across Name, Address, Email, Phone, and PropertyType simultaneously.
        /// </summary>
        public List<Customer> Search(string query)
        {
            var customers = new List<Customer>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "SELECT Id, Name, Address, Phone, Email, PropertyType FROM Customers " +
                "WHERE Name LIKE @Query OR Address LIKE @Query OR Email LIKE @Query " +
                "OR Phone LIKE @Query OR PropertyType LIKE @Query", connection);
            command.Parameters.AddWithValue("@Query", "%" + query + "%"); // wrap with wildcards
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                customers.Add(MapCustomer(reader));
            }
            return customers;
        }

        /// <summary>
        /// MapCustomer converts a single database row into a Customer object.
        /// reader.GetInt32(0) reads column index 0 as an integer (the Id column)
        /// reader.GetString(1) reads column index 1 as a string (the Name column)
        /// The column order matches the SELECT statement above.
        /// </summary>
        private Customer MapCustomer(SqlDataReader reader)
        {
            return new Customer(
                reader.GetInt32(0),  // Id
                reader.GetString(1), // Name
                reader.GetString(2), // Address
                reader.GetString(3), // Phone
                reader.GetString(4), // Email
                reader.GetString(5)  // PropertyType
            );
        }
    }
}