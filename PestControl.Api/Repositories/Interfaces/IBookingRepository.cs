using PestControl.Api.Models;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        List<Booking> GetAll();
        Booking GetById(int id);
        void Add(Booking booking);
        void Update(Booking booking);
        void Delete(int id);
        List<Booking> Search(string query);
    }
}
