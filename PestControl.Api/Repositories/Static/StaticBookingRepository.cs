using PestControl.Api.DataStructures;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Static
{
    public class StaticBookingRepository : IBookingRepository
    {
        private readonly BinarySearchTree<Booking> _tree;

        public StaticBookingRepository()
        {
            _tree = new BinarySearchTree<Booking>();
            _tree.Insert(1, new Booking(1, 1, 1, 1, "2026-03-25", "09:00", "Confirmed", "12 Oak Lane, London, E1 6AN", "Rat sighting in garden shed"));
            _tree.Insert(2, new Booking(2, 2, 3, 2, "2026-03-25", "11:00", "In Progress", "45 High Street, Manchester, M1 1AD", "Cockroach infestation in kitchen area"));
            _tree.Insert(3, new Booking(3, 3, 1, 1, "2026-03-26", "10:00", "Pending", "88 Victoria Road, Birmingham, B1 1BB", "Rat droppings found in storage room"));
            _tree.Insert(4, new Booking(4, 4, 4, 3, "2026-03-26", "14:00", "Confirmed", "3 Park Avenue, Leeds, LS1 1UR", "Bed bug report in bedroom"));
            _tree.Insert(5, new Booking(5, 5, 5, 2, "2026-03-27", "09:30", "Pending", "27 Church Street, Bristol, BS1 1HT", "Wasp nest under roof eaves"));
            _tree.Insert(6, new Booking(6, 6, 3, 4, "2026-03-27", "11:00", "Confirmed", "100 Queen Street, Edinburgh, EH2 1JE", "Routine cockroach inspection for hotel"));
            _tree.Insert(7, new Booking(7, 7, 7, 3, "2026-03-28", "10:00", "Pending", "9 Mill Lane, Cardiff, CF10 1FL", "Squirrel damage in loft space"));
            _tree.Insert(8, new Booking(8, 8, 6, 4, "2026-03-28", "13:00", "Confirmed", "55 Academy Road, Liverpool, L1 1JQ", "Pigeon nesting on school roof"));
            _tree.Insert(9, new Booking(9, 9, 2, 1, "2026-03-20", "09:00", "Completed", "71 Elm Close, Nottingham, NG1 1AB", "Mouse droppings in kitchen"));
            _tree.Insert(10, new Booking(10, 10, 1, 2, "2026-03-21", "14:00", "Completed", "200 Dock Road, London, E14 9TS", "Rat activity in warehouse loading bay"));
            _tree.Insert(11, new Booking(11, 1, 2, 1, "2026-03-18", "10:00", "Completed", "12 Oak Lane, London, E1 6AN", "Follow up mouse treatment"));
            _tree.Insert(12, new Booking(12, 3, 10, 2, "2026-03-29", "09:00", "Pending", "88 Victoria Road, Birmingham, B1 1BB", "Pharaoh ant sighting in restaurant kitchen"));
        }

        public List<Booking> GetAll()
        {
            return _tree.GetAll();
        }

        public Booking GetById(int id)
        {
            return _tree.Search(id);
        }

        public void Add(Booking booking)
        {
            booking.Id = _tree.Count() > 0 ? _tree.MaxKey() + 1 : 1;
            _tree.Insert(booking.Id, booking);
        }

        public void Update(Booking booking)
        {
            var existing = _tree.Search(booking.Id);
            if (existing != null)
            {
                existing.CustomerId = booking.CustomerId;
                existing.PestTypeId = booking.PestTypeId;
                existing.TechnicianId = booking.TechnicianId;
                existing.Date = booking.Date;
                existing.Time = booking.Time;
                existing.Status = booking.Status;
                existing.Location = booking.Location;
                existing.Notes = booking.Notes;
            }
        }

        public void Delete(int id)
        {
            _tree.Delete(id);
        }

        public List<Booking> Search(string query)
        {
            var lower = query.ToLower();
            return _tree.Filter(b =>
                b.Date.ToLower().Contains(lower) ||
                b.Time.ToLower().Contains(lower) ||
                b.Status.ToLower().Contains(lower) ||
                b.Location.ToLower().Contains(lower) ||
                b.Notes.ToLower().Contains(lower)
            );
        }
    }
}
