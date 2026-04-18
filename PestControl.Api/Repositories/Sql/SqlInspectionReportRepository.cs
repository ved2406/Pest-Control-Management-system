using Microsoft.Data.SqlClient;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Sql
{
    public class SqlInspectionReportRepository : IInspectionReportRepository
    {
        private readonly string _connectionString;

        public SqlInspectionReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<InspectionReport> GetAll()
        {
            var list = new List<InspectionReport>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, BookingId, Findings, Recommendations, FollowUpNeeded, ReportDate FROM InspectionReports", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        public InspectionReport GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, BookingId, Findings, Recommendations, FollowUpNeeded, ReportDate FROM InspectionReports WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public InspectionReport GetByBookingId(int bookingId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("SELECT Id, BookingId, Findings, Recommendations, FollowUpNeeded, ReportDate FROM InspectionReports WHERE BookingId = @BookingId", connection);
            command.Parameters.AddWithValue("@BookingId", bookingId);
            using var reader = command.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }

        public void Add(InspectionReport report)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "INSERT INTO InspectionReports (BookingId, Findings, Recommendations, FollowUpNeeded, ReportDate) OUTPUT INSERTED.Id VALUES (@BookingId, @Findings, @Recommendations, @FollowUpNeeded, @ReportDate)", connection);
            command.Parameters.AddWithValue("@BookingId", report.BookingId);
            command.Parameters.AddWithValue("@Findings", report.Findings);
            command.Parameters.AddWithValue("@Recommendations", report.Recommendations);
            command.Parameters.AddWithValue("@FollowUpNeeded", report.FollowUpNeeded);
            command.Parameters.AddWithValue("@ReportDate", report.ReportDate);
            report.Id = (int)command.ExecuteScalar();
        }

        public void Update(InspectionReport report)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "UPDATE InspectionReports SET BookingId=@BookingId, Findings=@Findings, Recommendations=@Recommendations, FollowUpNeeded=@FollowUpNeeded, ReportDate=@ReportDate WHERE Id=@Id", connection);
            command.Parameters.AddWithValue("@Id", report.Id);
            command.Parameters.AddWithValue("@BookingId", report.BookingId);
            command.Parameters.AddWithValue("@Findings", report.Findings);
            command.Parameters.AddWithValue("@Recommendations", report.Recommendations);
            command.Parameters.AddWithValue("@FollowUpNeeded", report.FollowUpNeeded);
            command.Parameters.AddWithValue("@ReportDate", report.ReportDate);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand("DELETE FROM InspectionReports WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }

        public List<InspectionReport> Search(string query)
        {
            var list = new List<InspectionReport>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(
                "SELECT Id, BookingId, Findings, Recommendations, FollowUpNeeded, ReportDate FROM InspectionReports WHERE Findings LIKE @Q OR Recommendations LIKE @Q OR ReportDate LIKE @Q", connection);
            command.Parameters.AddWithValue("@Q", "%" + query + "%");
            using var reader = command.ExecuteReader();
            while (reader.Read()) list.Add(Map(reader));
            return list;
        }

        private InspectionReport Map(SqlDataReader reader)
        {
            return new InspectionReport(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetBoolean(4), reader.GetString(5));
        }
    }
}
