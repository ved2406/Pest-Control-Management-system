using Microsoft.Data.SqlClient;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Sql
{
    public class SqlCustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public SqlCustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Customer> GetAll()
        {
            var customers = new List<Customer>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, Name, Address, Phone, Email, PropertyType FROM Customers", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                customers.Add(MapCustomer(reader));
            }
            return customers;
        }

        public Customer GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "SELECT Id, Name, Address, Phone, Email, PropertyType FROM Customers WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapCustomer(reader);
            }
            return null;
        }

        public void Add(Customer customer)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "INSERT INTO Customers (Name, Address, Phone, Email, PropertyType) " +
                "OUTPUT INSERTED.Id " +
                "VALUES (@Name, @Address, @Phone, @Email, @PropertyType)", connection);
            command.Parameters.AddWithValue("@Name", customer.Name);
            command.Parameters.AddWithValue("@Address", customer.Address);
            command.Parameters.AddWithValue("@Phone", customer.Phone);
            command.Parameters.AddWithValue("@Email", customer.Email);
            command.Parameters.AddWithValue("@PropertyType", customer.PropertyType);
            customer.Id = (int)command.ExecuteScalar();
        }

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
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("DELETE FROM Customers WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }

        public List<Customer> Search(string query)
        {
            var customers = new List<Customer>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "SELECT Id, Name, Address, Phone, Email, PropertyType FROM Customers " +
                "WHERE Name LIKE @Query OR Address LIKE @Query OR Email LIKE @Query " +
                "OR Phone LIKE @Query OR PropertyType LIKE @Query", connection);
            command.Parameters.AddWithValue("@Query", "%" + query + "%");
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                customers.Add(MapCustomer(reader));
            }
            return customers;
        }

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