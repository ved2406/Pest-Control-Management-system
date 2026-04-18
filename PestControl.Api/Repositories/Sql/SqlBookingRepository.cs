using Microsoft.Data.SqlClient;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Sql
{
    public class SqlBookingRepository : IBookingRepository
    {
        private readonly string _connectionString;

        public SqlBookingRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Booking> GetAll()
        {
            var list = new List<Booking>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, CustomerId, PestTypeId, TechnicianId, Date, Time, Status, Location, Notes FROM Bookings", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        public Booking GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, CustomerId, PestTypeId, TechnicianId, Date, Time, Status, Location, Notes FROM Bookings WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public void Add(Booking booking)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "INSERT INTO Bookings (CustomerId, PestTypeId, TechnicianId, Date, Time, Status, Location, Notes) OUTPUT INSERTED.Id VALUES (@CustomerId, @PestTypeId, @TechnicianId, @Date, @Time, @Status, @Location, @Notes)", connection);
            command.Parameters.AddWithValue("@CustomerId", booking.CustomerId);
            command.Parameters.AddWithValue("@PestTypeId", booking.PestTypeId);
            command.Parameters.AddWithValue("@TechnicianId", booking.TechnicianId);
            command.Parameters.AddWithValue("@Date", booking.Date);
            command.Parameters.AddWithValue("@Time", booking.Time);
            command.Parameters.AddWithValue("@Status", booking.Status);
            command.Parameters.AddWithValue("@Location", booking.Location);
            command.Parameters.AddWithValue("@Notes", booking.Notes);
            booking.Id = (int)command.ExecuteScalar();
        }

        public void Update(Booking booking)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "UPDATE Bookings SET CustomerId=@CustomerId, PestTypeId=@PestTypeId, TechnicianId=@TechnicianId, Date=@Date, Time=@Time, Status=@Status, Location=@Location, Notes=@Notes WHERE Id=@Id", connection);
            command.Parameters.AddWithValue("@Id", booking.Id);
            command.Parameters.AddWithValue("@CustomerId", booking.CustomerId);
            command.Parameters.AddWithValue("@PestTypeId", booking.PestTypeId);
            command.Parameters.AddWithValue("@TechnicianId", booking.TechnicianId);
            command.Parameters.AddWithValue("@Date", booking.Date);
            command.Parameters.AddWithValue("@Time", booking.Time);
            command.Parameters.AddWithValue("@Status", booking.Status);
            command.Parameters.AddWithValue("@Location", booking.Location);
            command.Parameters.AddWithValue("@Notes", booking.Notes);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("DELETE FROM Bookings WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }

        public List<Booking> Search(string query)
        {
            var list = new List<Booking>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "SELECT Id, CustomerId, PestTypeId, TechnicianId, Date, Time, Status, Location, Notes FROM Bookings WHERE Date LIKE @Q OR Status LIKE @Q OR Location LIKE @Q OR Notes LIKE @Q", connection);
            command.Parameters.AddWithValue("@Q", "%" + query + "%");
            using var reader = command.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        private Booking Map(SqlDataReader reader)
        {
            return new Booking(
                reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3),
                reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetString(8));
        }
    }
}
