using PestControl.Api.Models;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Interfaces
{
    public interface IInspectionReportRepository
    {
        List<InspectionReport> GetAll();
        InspectionReport GetById(int id);
        InspectionReport GetByBookingId(int bookingId);
        void Add(InspectionReport report);
        void Update(InspectionReport report);
        void Delete(int id);
        List<InspectionReport> Search(string query);
    }
}
