using Microsoft.Data.SqlClient;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Sql
{
    public class SqlTreatmentRepository : ITreatmentRepository
    {
        private readonly string _connectionString;

        public SqlTreatmentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Treatment> GetAll()
        {
            var list = new List<Treatment>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, ProductName, Method, TargetPestTypeId, SafetyInfo FROM Treatments", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        public Treatment GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, ProductName, Method, TargetPestTypeId, SafetyInfo FROM Treatments WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public void Add(Treatment treatment)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "INSERT INTO Treatments (ProductName, Method, TargetPestTypeId, SafetyInfo) OUTPUT INSERTED.Id VALUES (@ProductName, @Method, @TargetPestTypeId, @SafetyInfo)", connection);
            command.Parameters.AddWithValue("@ProductName", treatment.ProductName);
            command.Parameters.AddWithValue("@Method", treatment.Method);
            command.Parameters.AddWithValue("@TargetPestTypeId", treatment.TargetPestTypeId);
            command.Parameters.AddWithValue("@SafetyInfo", treatment.SafetyInfo);
            treatment.Id = (int)command.ExecuteScalar();
        }

        public void Update(Treatment treatment)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "UPDATE Treatments SET ProductName=@ProductName, Method=@Method, TargetPestTypeId=@TargetPestTypeId, SafetyInfo=@SafetyInfo WHERE Id=@Id", connection);
            command.Parameters.AddWithValue("@Id", treatment.Id);
            command.Parameters.AddWithValue("@ProductName", treatment.ProductName);
            command.Parameters.AddWithValue("@Method", treatment.Method);
            command.Parameters.AddWithValue("@TargetPestTypeId", treatment.TargetPestTypeId);
            command.Parameters.AddWithValue("@SafetyInfo", treatment.SafetyInfo);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("DELETE FROM Treatments WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }

        public List<Treatment> Search(string query)
        {
            var list = new List<Treatment>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "SELECT Id, ProductName, Method, TargetPestTypeId, SafetyInfo FROM Treatments WHERE ProductName LIKE @Q OR Method LIKE @Q OR SafetyInfo LIKE @Q", connection);
            command.Parameters.AddWithValue("@Q", "%" + query + "%");
            using var reader = command.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        private Treatment Map(SqlDataReader reader)
        {
            return new Treatment(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3), reader.GetString(4));
        }
    }
}
