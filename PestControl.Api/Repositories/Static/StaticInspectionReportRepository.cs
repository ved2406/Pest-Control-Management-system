using PestControl.Api.DataStructures;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Static
{
    public class StaticInspectionReportRepository : IInspectionReportRepository
    {
        private readonly BinarySearchTree<InspectionReport> _tree;

        public StaticInspectionReportRepository()
        {
            _tree = new BinarySearchTree<InspectionReport>();
            _tree.Insert(1, new InspectionReport(1, 9, "Mouse droppings found behind kitchen units and under sink. Entry point identified at pipe gap in external wall. No live mice observed during visit.", "Seal pipe gap with wire wool and cement. Install bait stations in kitchen and utility room. Follow up in 2 weeks.", true, "2026-03-20"));
            _tree.Insert(2, new InspectionReport(2, 10, "Active rat burrow found near loading bay drainage. Gnaw marks on stored pallets. Droppings consistent with brown rat activity over several weeks.", "Deploy tamper-resistant bait stations at 5 locations. Repair damaged drain cover. Remove harbourage around perimeter. Monthly monitoring contract recommended.", true, "2026-03-21"));
            _tree.Insert(3, new InspectionReport(3, 11, "Follow-up visit. No new mouse activity detected. Bait stations undisturbed. Entry point has been sealed by property owner as recommended.", "Remove bait stations after next clear visit. No further action required if activity remains absent.", false, "2026-03-18"));
        }

        public List<InspectionReport> GetAll()
        {
            return _tree.GetAll();
        }

        public InspectionReport GetById(int id)
        {
            return _tree.Search(id);
        }

        public InspectionReport GetByBookingId(int bookingId)
        {
            var matches = _tree.Filter(r => r.BookingId == bookingId);
            return matches.Count > 0 ? matches[0] : null;
        }

        public void Add(InspectionReport report)
        {
            report.Id = _tree.Count() > 0 ? _tree.MaxKey() + 1 : 1;
            _tree.Insert(report.Id, report);
        }

        public void Update(InspectionReport report)
        {
            var existing = _tree.Search(report.Id);
            if (existing != null)
            {
                existing.BookingId = report.BookingId;
                existing.Findings = report.Findings;
                existing.Recommendations = report.Recommendations;
                existing.FollowUpNeeded = report.FollowUpNeeded;
                existing.ReportDate = report.ReportDate;
            }
        }

        public void Delete(int id)
        {
            _tree.Delete(id);
        }

        public List<InspectionReport> Search(string query)
        {
            var lower = query.ToLower();
            return _tree.Filter(r =>
                r.Findings.ToLower().Contains(lower) ||
                r.Recommendations.ToLower().Contains(lower) ||
                r.ReportDate.Contains(lower)
            );
        }
    }
}
