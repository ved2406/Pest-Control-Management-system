using Microsoft.Data.SqlClient;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Sql
{
    public class SqlPestTypeRepository : IPestTypeRepository
    {
        private readonly string _connectionString;

        public SqlPestTypeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<PestType> GetAll()
        {
            var list = new List<PestType>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, Name, Category, Description, RiskLevel FROM PestTypes", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(Map(reader));
            }
            return list;
        }

        public PestType GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, Name, Category, Description, RiskLevel FROM PestTypes WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public void Add(PestType pestType)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "INSERT INTO PestTypes (Name, Category, Description, RiskLevel) OUTPUT INSERTED.Id VALUES (@Name, @Category, @Description, @RiskLevel)", connection);
            command.Parameters.AddWithValue("@Name", pestType.Name);
            command.Parameters.AddWithValue("@Category", pestType.Category);
            command.Parameters.AddWithValue("@Description", pestType.Description);
            command.Parameters.AddWithValue("@RiskLevel", pestType.RiskLevel);
            pestType.Id = (int)command.ExecuteScalar();
        }

        public void Update(PestType pestType)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "UPDATE PestTypes SET Name = @Name, Category = @Category, Description = @Description, RiskLevel = @RiskLevel WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", pestType.Id);
            command.Parameters.AddWithValue("@Name", pestType.Name);
            command.Parameters.AddWithValue("@Category", pestType.Category);
            command.Parameters.AddWithValue("@Description", pestType.Description);
            command.Parameters.AddWithValue("@RiskLevel", pestType.RiskLevel);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("DELETE FROM PestTypes WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }

        public List<PestType> Search(string query)
        {
            var list = new List<PestType>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "SELECT Id, Name, Category, Description, RiskLevel FROM PestTypes WHERE Name LIKE @Q OR Category LIKE @Q OR Description LIKE @Q OR RiskLevel LIKE @Q", connection);
            command.Parameters.AddWithValue("@Q", "%" + query + "%");
            using var reader = command.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        private PestType Map(SqlDataReader reader)
        {
            return new PestType(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4));
        }
    }
}
