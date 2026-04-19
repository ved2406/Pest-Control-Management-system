using PestControl.Api.Models;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Interfaces
{
    /// <summary>
    /// Defines what booking operations we can do — just what's available, not how it's done.
    /// Could be talking to a real database or using test data, the interface doesn't care.
    /// </summary>
    public interface IBookingRepository
    {
        // Get all bookings
        List<Booking> GetAll();

        // Get a specific booking by ID — returns null if it doesn't exist
        Booking GetById(int id);

        // Create a new booking
        void Add(Booking booking);

        // Update an existing booking
        void Update(Booking booking);

        // Delete a booking
        void Delete(int id);

        // Find bookings by searching through relevant fields
        List<Booking> Search(string query);
    }
}
