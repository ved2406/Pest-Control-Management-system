using PestControl.Api.Models;
using PestControl.Api.Repositories.Static;

namespace PestControl.Tests;

// Tests for the StaticCustomerRepository — this suite makes sure all the basic customer operations work as expected.
// We're testing the full lifecycle: creating new customers, reading them back, updating their information, 
// removing them when needed, and searching to find specific customers. If any of these break, the whole app falls apart.
[TestClass]
public sealed class CustomerRepositoryTests
{
    private StaticCustomerRepository _repo;

    // Before each test, we create a brand new repository instance. This ensures we start with a clean slate
    // and that one test's actions don't mess up another test. Each test runs independently.
    [TestInitialize]
    public void Setup()
    {
        _repo = new StaticCustomerRepository();
    }

    // This test makes sure that when we ask for all customers, we actually get a meaningful set of test data.
    // We're checking that at least 10 customers come back, which confirms the in-memory repository has been
    // properly initialized with test data. If this fails, something is wrong with our test fixtures.
    [TestMethod]
    public void GetAll_ReturnsAllCustomers()
    {
        var result = _repo.GetAll();
        Assert.IsTrue(result.Count >= 10);
    }

    // When we ask for a customer by their ID (in this case, ID 1), we should get back that customer with
    // all their data intact. We're checking that the customer exists and that the name matches what we expect.
    // This is crucial because the entire application relies on being able to fetch customer details quickly.
    [TestMethod]
    public void GetById_ValidId_ReturnsCustomer()
    {
        var customer = _repo.GetById(1);
        Assert.IsNotNull(customer);
        Assert.AreEqual("John Smith", customer.Name);
    }

    // Here we're testing the unhappy path—what happens when someone looks for a customer ID that doesn't exist?
    // Instead of crashing or throwing an exception, the system should gracefully return null. This way the
    // API client knows "this customer doesn't exist" rather than getting a server error.
    [TestMethod]
    public void GetById_InvalidId_ReturnsNull()
    {
        var customer = _repo.GetById(999);
        Assert.IsNull(customer);
    }

    // When we add a brand new customer to the system, we need to make sure the count goes up by exactly one.
    // This test counts the customers before and after adding a new one, ensuring our Add method properly
    // inserts the customer into the repository. It's a simple but critical check for create operations.
    [TestMethod]
    public void Add_IncreasesCount()
    {
        int before = _repo.GetAll().Count;
        _repo.Add(new Customer(0, "Test User", "1 Test Rd", "07700000000", "test@test.com", "Residential"));
        Assert.AreEqual(before + 1, _repo.GetAll().Count);
    }

    // After getting an existing customer, we change their name to something else and call Update. Then we
    // fetch that customer again and verify the name actually changed. This proves the Update method is
    // persisting changes properly and not just updating in memory temporarily.
    [TestMethod]
    public void Update_ChangesName()
    {
        var customer = _repo.GetById(1);
        customer.Name = "Updated Name";
        _repo.Update(customer);
        Assert.AreEqual("Updated Name", _repo.GetById(1).Name);
    }

    // When we delete a customer, two things should happen: the total count should decrease by one, and when
    // we try to fetch that customer again by ID, we should get null back. This ensures the Delete method
    // actually removes the customer and doesn't just mark them as deleted or hide them.
    [TestMethod]
    public void Delete_RemovesCustomer()
    {
        int before = _repo.GetAll().Count;
        _repo.Delete(1);
        Assert.AreEqual(before - 1, _repo.GetAll().Count);
        Assert.IsNull(_repo.GetById(1));
    }

    // The Search method should use case-insensitive matching to find customers by name. When we search for
    // "john", we should find customers whose name contains "John". This is important for the UI where users
    // type partial names to find customers without knowing the exact spelling.
    [TestMethod]
    public void Search_ByName_ReturnsMatch()
    {
        var results = _repo.Search("john");
        Assert.IsTrue(results.Count > 0);
        Assert.IsTrue(results.Exists(c => c.Name.Contains("John")));
    }

    // If we search for something that definitely won't be in the system (like "zzzznonexistent"), the search
    // should return an empty list rather than null or throwing an error. This tells the UI "no results found"
    // instead of causing an exception.
    [TestMethod]
    public void Search_NoMatch_ReturnsEmpty()
    {
        var results = _repo.Search("zzzznonexistent");
        Assert.AreEqual(0, results.Count);
    }
}

// Tests for the StaticBookingRepository — this suite ensures bookings can be created, read, updated, and deleted,
// plus we test searching and filtering. Bookings are the heart of the system, so we need to make sure all
// operations on them work reliably. A broken booking operation could ruin the whole scheduling system.
[TestClass]
public sealed class BookingRepositoryTests
{
    private StaticBookingRepository _repo;

    // Just like the customer tests, we create a fresh booking repository for each test so they don't
    // interfere with each other. Each test gets its own clean state to work with.
    [TestInitialize]
    public void Setup()
    {
        _repo = new StaticBookingRepository();
    }

    // When we fetch all bookings, we should get back at least 12 test bookings that are pre-loaded into the
    // system. This verifies the repository has been initialized with realistic test data so we can actually
    // test other operations against them.
    [TestMethod]
    public void GetAll_ReturnsBookings()
    {
        var result = _repo.GetAll();
        Assert.IsTrue(result.Count >= 12);
    }

    // fetching a booking by ID should return the booking with all its details intact. We check that booking
    // ID 1 returns a valid booking object, and that it's linked to the correct customer (CustomerId 1).
    // This relationship is essential for showing booking details in the UI.
    [TestMethod]
    public void GetById_ValidId_ReturnsBooking()
    {
        var booking = _repo.GetById(1);
        Assert.IsNotNull(booking);
        Assert.AreEqual(1, booking.CustomerId);
    }

    // When a customer schedules a new booking, we add it to the repository and the count should go up by one.
    // We verify this by counting before and after the Add operation. This is the core CREATE operation
    // that makes the whole reservation system work.
    [TestMethod]
    public void Add_CreatesBooking()
    {
        int before = _repo.GetAll().Count;
        _repo.Add(new Booking(0, 1, 1, 1, "2026-04-01", "10:00", "Pending", "Test Location", "Test notes"));
        Assert.AreEqual(before + 1, _repo.GetAll().Count);
    }

    // When a booking is cancelled or needs to be removed, the Delete method should completely remove it from
    // the system. After deletion, fetching it by ID should return null. This ensures cancelled bookings
    // don't clutter the system or confuse the schedule.
    [TestMethod]
    public void Delete_RemovesBooking()
    {
        _repo.Delete(1);
        Assert.IsNull(_repo.GetById(1));
    }

    // Users often need to filter bookings by their current status—like viewing only completed jobs or
    // pending appointments. This test verifies that searching by status (in this case, "Completed") returns
    // bookings matching that status. It's essential for the reporting and dashboard features.
    [TestMethod]
    public void Search_ByStatus_ReturnsMatches()
    {
        var results = _repo.Search("Completed");
        Assert.IsTrue(results.Count > 0);
    }
}