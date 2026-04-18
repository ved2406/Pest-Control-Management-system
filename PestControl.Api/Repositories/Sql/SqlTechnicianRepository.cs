using Microsoft.Data.SqlClient;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Sql
{
    public class SqlTechnicianRepository : ITechnicianRepository
    {
        private readonly string _connectionString;

        public SqlTechnicianRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Technician> GetAll()
        {
            var list = new List<Technician>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, Name, Specialisation, Phone, Email, Available FROM Technicians", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        public Technician GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, Name, Specialisation, Phone, Email, Available FROM Technicians WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public void Add(Technician technician)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "INSERT INTO Technicians (Name, Specialisation, Phone, Email, Available) OUTPUT INSERTED.Id VALUES (@Name, @Specialisation, @Phone, @Email, @Available)", connection);
            command.Parameters.AddWithValue("@Name", technician.Name);
            command.Parameters.AddWithValue("@Specialisation", technician.Specialisation);
            command.Parameters.AddWithValue("@Phone", technician.Phone);
            command.Parameters.AddWithValue("@Email", technician.Email);
            command.Parameters.AddWithValue("@Available", technician.Available);
            technician.Id = (int)command.ExecuteScalar();
        }

        public void Update(Technician technician)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "UPDATE Technicians SET Name=@Name, Specialisation=@Specialisation, Phone=@Phone, Email=@Email, Available=@Available WHERE Id=@Id", connection);
            command.Parameters.AddWithValue("@Id", technician.Id);
            command.Parameters.AddWithValue("@Name", technician.Name);
            command.Parameters.AddWithValue("@Specialisation", technician.Specialisation);
            command.Parameters.AddWithValue("@Phone", technician.Phone);
            command.Parameters.AddWithValue("@Email", technician.Email);
            command.Parameters.AddWithValue("@Available", technician.Available);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("DELETE FROM Technicians WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }

        public List<Technician> Search(string query)
        {
            var list = new List<Technician>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "SELECT Id, Name, Specialisation, Phone, Email, Available FROM Technicians WHERE Name LIKE @Q OR Specialisation LIKE @Q OR Email LIKE @Q OR Phone LIKE @Q", connection);
            command.Parameters.AddWithValue("@Q", "%" + query + "%");
            using var reader = command.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        private Technician Map(SqlDataReader reader)
        {
            return new Technician(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetBoolean(5));
        }
    }
}
