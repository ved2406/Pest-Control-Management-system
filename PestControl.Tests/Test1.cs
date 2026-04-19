using PestControl.Api.DataStructures;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Static;
using PestControl.Api.Repositories.Interfaces;
using PestControl.Api.Services;

namespace PestControl.Tests;

[TestClass]
public sealed class CustomerRepositoryTests
{
    private StaticCustomerRepository _repo;

    [TestInitialize]
    public void Setup()
    {
        _repo = new StaticCustomerRepository();
    }

    [TestMethod]
    public void GetAll_ReturnsAllCustomers()
    {
        var result = _repo.GetAll();
        Assert.IsTrue(result.Count >= 10);
    }

    [TestMethod]
    public void GetById_ValidId_ReturnsCustomer()
    {
        var customer = _repo.GetById(1);
        Assert.IsNotNull(customer);
        Assert.AreEqual("John Smith", customer.Name);
    }

    [TestMethod]
    public void GetById_InvalidId_ReturnsNull()
    {
        var customer = _repo.GetById(999);
        Assert.IsNull(customer);
    }

    [TestMethod]
    public void Add_IncreasesCount()
    {
        int before = _repo.GetAll().Count;
        _repo.Add(new Customer(0, "Test User", "1 Test Rd", "07700000000", "test@test.com", "Residential"));
        Assert.AreEqual(before + 1, _repo.GetAll().Count);
    }

    [TestMethod]
    public void Update_ChangesName()
    {
        var customer = _repo.GetById(1);
        customer.Name = "Updated Name";
        _repo.Update(customer);
        Assert.AreEqual("Updated Name", _repo.GetById(1).Name);
    }

    [TestMethod]
    public void Delete_RemovesCustomer()
    {
        int before = _repo.GetAll().Count;
        _repo.Delete(1);
        Assert.AreEqual(before - 1, _repo.GetAll().Count);
        Assert.IsNull(_repo.GetById(1));
    }

    [TestMethod]
    public void Search_ByName_ReturnsMatch()
    {
        var results = _repo.Search("john");
        Assert.IsTrue(results.Count > 0);
        Assert.IsTrue(results.Exists(c => c.Name.Contains("John")));
    }

    [TestMethod]
    public void Search_NoMatch_ReturnsEmpty()
    {
        var results = _repo.Search("zzzznonexistent");
        Assert.AreEqual(0, results.Count);
    }
}

[TestClass]
public sealed class BookingRepositoryTests
{
    private StaticBookingRepository _repo;

    [TestInitialize]
    public void Setup()
    {
        _repo = new StaticBookingRepository();
    }

    [TestMethod]
    public void GetAll_ReturnsBookings()
    {
        var result = _repo.GetAll();
        Assert.IsTrue(result.Count >= 12);
    }

    [TestMethod]
    public void GetById_ValidId_ReturnsBooking()
    {
        var booking = _repo.GetById(1);
        Assert.IsNotNull(booking);
        Assert.AreEqual(1, booking.CustomerId);
    }

    [TestMethod]
    public void Add_CreatesBooking()
    {
        int before = _repo.GetAll().Count;
        _repo.Add(new Booking(0, 1, 1, 1, "2026-04-01", "10:00", "Pending", "Test Location", "Test notes"));
        Assert.AreEqual(before + 1, _repo.GetAll().Count);
    }

    [TestMethod]
    public void Delete_RemovesBooking()
    {
        _repo.Delete(1);
        Assert.IsNull(_repo.GetById(1));
    }

    [TestMethod]
    public void Search_ByStatus_ReturnsMatches()
    {
        var results = _repo.Search("Completed");
        Assert.IsTrue(results.Count > 0);
    }
}

[TestClass]
public sealed class PestTypeRepositoryTests
{
    private StaticPestTypeRepository _repo;

    [TestInitialize]
    public void Setup()
    {
        _repo = new StaticPestTypeRepository();
    }

    [TestMethod]
    public void GetAll_ReturnsPestTypes()
    {
        Assert.IsTrue(_repo.GetAll().Count >= 12);
    }

    [TestMethod]
    public void GetById_ReturnsCorrectPest()
    {
        var pest = _repo.GetById(1);
        Assert.AreEqual("Brown Rat", pest.Name);
    }

    [TestMethod]
    public void Search_ByCategory_ReturnsMatches()
    {
        var results = _repo.Search("Rodents");
        Assert.IsTrue(results.Count >= 2);
    }

    [TestMethod]
    public void Search_ByRiskLevel_ReturnsMatches()
    {
        var results = _repo.Search("High");
        Assert.IsTrue(results.Count > 0);
    }
}

[TestClass]
public sealed class TechnicianRepositoryTests
{
    private StaticTechnicianRepository _repo;

    [TestInitialize]
    public void Setup()
    {
        _repo = new StaticTechnicianRepository();
    }

    [TestMethod]
    public void GetAll_ReturnsTechnicians()
    {
        Assert.AreEqual(4, _repo.GetAll().Count);
    }

    [TestMethod]
    public void Search_BySpecialisation_ReturnsMatch()
    {
        var results = _repo.Search("Rodents");
        Assert.IsTrue(results.Count > 0);
    }
}

[TestClass]
public sealed class TreatmentRepositoryTests
{
    private StaticTreatmentRepository _repo;

    [TestInitialize]
    public void Setup()
    {
        _repo = new StaticTreatmentRepository();
    }

    [TestMethod]
    public void GetAll_ReturnsTreatments()
    {
        Assert.IsTrue(_repo.GetAll().Count >= 10);
    }

    [TestMethod]
    public void Search_ByMethod_ReturnsMatch()
    {
        var results = _repo.Search("Bait");
        Assert.IsTrue(results.Count > 0);
    }
}

[TestClass]
public sealed class InspectionReportRepositoryTests
{
    private StaticInspectionReportRepository _repo;

    [TestInitialize]
    public void Setup()
    {
        _repo = new StaticInspectionReportRepository();
    }

    [TestMethod]
    public void GetAll_ReturnsReports()
    {
        Assert.IsTrue(_repo.GetAll().Count >= 3);
    }

    [TestMethod]
    public void GetByBookingId_ReturnsCorrectReport()
    {
        var report = _repo.GetByBookingId(9);
        Assert.IsNotNull(report);
        Assert.IsTrue(report.Findings.Contains("Mouse"));
    }

    [TestMethod]
    public void Search_ByFindings_ReturnsMatch()
    {
        var results = _repo.Search("rat");
        Assert.IsTrue(results.Count > 0);
    }
}

[TestClass]
public sealed class SearchServiceTests
{
    private SearchService _service;

    [TestInitialize]
    public void Setup()
    {
        _service = new SearchService(
            new StaticCustomerRepository(),
            new StaticPestTypeRepository(),
            new StaticBookingRepository(),
            new StaticTechnicianRepository(),
            new StaticTreatmentRepository(),
            new StaticInspectionReportRepository()
        );
    }

    [TestMethod]
    public void Search_EmptyQuery_ReturnsEmpty()
    {
        var results = _service.Search("");
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void Search_Rat_ReturnsMultipleCategories()
    {
        var results = _service.Search("rat");
        Assert.IsTrue(results.Count > 0);
        var categories = results.Select(r => r.Category).Distinct().ToList();
        Assert.IsTrue(categories.Count > 1, "Should match across multiple data categories");
    }

    [TestMethod]
    public void Search_CustomerName_ReturnsCustomerResult()
    {
        var results = _service.Search("John Smith");
        Assert.IsTrue(results.Exists(r => r.Category == "Customer"));
    }
}

[TestClass]
public sealed class BinarySearchTreeTests
{
    private BinarySearchTree<string> _tree;

    [TestInitialize]
    public void Setup()
    {
        _tree = new BinarySearchTree<string>();
    }

    [TestMethod]
    public void Insert_And_Search_ReturnsValue()
    {
        _tree.Insert(5, "five");
        Assert.AreEqual("five", _tree.Search(5));
    }

    [TestMethod]
    public void Search_NonExistentKey_ReturnsNull()
    {
        _tree.Insert(1, "one");
        Assert.IsNull(_tree.Search(99));
    }

    [TestMethod]
    public void Insert_MultipleItems_AllRetrievable()
    {
        _tree.Insert(3, "three");
        _tree.Insert(1, "one");
        _tree.Insert(5, "five");
        _tree.Insert(2, "two");
        _tree.Insert(4, "four");

        Assert.AreEqual("one", _tree.Search(1));
        Assert.AreEqual("two", _tree.Search(2));
        Assert.AreEqual("three", _tree.Search(3));
        Assert.AreEqual("four", _tree.Search(4));
        Assert.AreEqual("five", _tree.Search(5));
    }

    [TestMethod]
    public void GetAll_ReturnsSortedByKey()
    {
        _tree.Insert(5, "five");
        _tree.Insert(1, "one");
        _tree.Insert(3, "three");
        _tree.Insert(2, "two");
        _tree.Insert(4, "four");

        var all = _tree.GetAll();
        Assert.AreEqual(5, all.Count);
        Assert.AreEqual("one", all[0]);
        Assert.AreEqual("two", all[1]);
        Assert.AreEqual("three", all[2]);
        Assert.AreEqual("four", all[3]);
        Assert.AreEqual("five", all[4]);
    }

    [TestMethod]
    public void Delete_LeafNode_RemovesIt()
    {
        _tree.Insert(2, "two");
        _tree.Insert(1, "one");
        _tree.Insert(3, "three");

        _tree.Delete(1);
        Assert.IsNull(_tree.Search(1));
        Assert.AreEqual(2, _tree.Count());
    }

    [TestMethod]
    public void Delete_NodeWithOneChild_RemovesCorrectly()
    {
        _tree.Insert(3, "three");
        _tree.Insert(1, "one");
        _tree.Insert(2, "two");

        _tree.Delete(1);
        Assert.IsNull(_tree.Search(1));
        Assert.AreEqual("two", _tree.Search(2));
    }

    [TestMethod]
    public void Delete_NodeWithTwoChildren_RemovesCorrectly()
    {
        _tree.Insert(5, "five");
        _tree.Insert(3, "three");
        _tree.Insert(7, "seven");
        _tree.Insert(2, "two");
        _tree.Insert(4, "four");

        _tree.Delete(3);
        Assert.IsNull(_tree.Search(3));
        Assert.AreEqual("two", _tree.Search(2));
        Assert.AreEqual("four", _tree.Search(4));
        Assert.AreEqual(4, _tree.Count());
    }

    [TestMethod]
    public void Delete_RootNode_TreeStillWorks()
    {
        _tree.Insert(5, "five");
        _tree.Insert(3, "three");
        _tree.Insert(7, "seven");

        _tree.Delete(5);
        Assert.IsNull(_tree.Search(5));
        Assert.AreEqual("three", _tree.Search(3));
        Assert.AreEqual("seven", _tree.Search(7));
    }

    [TestMethod]
    public void Count_ReturnsCorrectCount()
    {
        Assert.AreEqual(0, _tree.Count());
        _tree.Insert(1, "one");
        Assert.AreEqual(1, _tree.Count());
        _tree.Insert(2, "two");
        Assert.AreEqual(2, _tree.Count());
        _tree.Delete(1);
        Assert.AreEqual(1, _tree.Count());
    }

    [TestMethod]
    public void MaxKey_ReturnsLargestKey()
    {
        _tree.Insert(3, "three");
        _tree.Insert(1, "one");
        _tree.Insert(7, "seven");
        _tree.Insert(5, "five");

        Assert.AreEqual(7, _tree.MaxKey());
    }

    [TestMethod]
    public void Filter_ReturnsMatchingValues()
    {
        _tree.Insert(1, "apple");
        _tree.Insert(2, "banana");
        _tree.Insert(3, "apricot");
        _tree.Insert(4, "blueberry");

        var results = _tree.Filter(v => v.StartsWith("ap"));
        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(results.Contains("apple"));
        Assert.IsTrue(results.Contains("apricot"));
    }

    [TestMethod]
    public void Insert_DuplicateKey_UpdatesValue()
    {
        _tree.Insert(1, "original");
        _tree.Insert(1, "updated");
        Assert.AreEqual("updated", _tree.Search(1));
        Assert.AreEqual(1, _tree.Count());
    }

    [TestMethod]
    public void Delete_NonExistentKey_DoesNothing()
    {
        _tree.Insert(1, "one");
        _tree.Delete(999);
        Assert.AreEqual(1, _tree.Count());
    }
}

[TestClass]
public sealed class PestControlAgentTests
{
    private PestControlAgent _agent;

    [TestInitialize]
    public void Setup()
    {
        _agent = new PestControlAgent(
            new StaticCustomerRepository(),
            new StaticPestTypeRepository(),
            new StaticBookingRepository(),
            new StaticTechnicianRepository(),
            new StaticTreatmentRepository(),
            new StaticInspectionReportRepository()
        );
    }

    [TestMethod]
    public void GetArms_ReturnsSevenArms()
    {
        var arms = _agent.GetArms();
        Assert.AreEqual(7, arms.Count);
    }

    [TestMethod]
    public void Process_EmptyMessage_ReturnsGreeting()
    {
        var response = _agent.Process("");
        Assert.AreEqual("general", response.Arm);
        Assert.IsTrue(response.Message.Contains("PestPro AI Assistant"));
    }

    [TestMethod]
    public void Process_HelloGreeting_ReturnsCapabilities()
    {
        var response = _agent.Process("hello");
        Assert.AreEqual("general", response.Arm);
        Assert.IsTrue(response.Message.Contains("Search for customers"));
    }

    [TestMethod]
    public void Process_FindCustomer_UsesCustomerArm()
    {
        var response = _agent.Process("find customer John");
        Assert.AreEqual("CustomerSearch", response.Arm);
        Assert.IsTrue(response.Message.Contains("John") || response.Message.Contains("customer"));
    }

    [TestMethod]
    public void Process_CustomerSearch_AllCustomers()
    {
        var response = _agent.Process("show me all customers");
        Assert.AreEqual("CustomerSearch", response.Arm);
        Assert.IsTrue(response.Message.Contains("customer"));
    }

    [TestMethod]
    public void Process_TechnicianAvailability_UsesCorrectArm()
    {
        var response = _agent.Process("show available technicians");
        Assert.AreEqual("TechnicianAvailability", response.Arm);
        Assert.IsTrue(response.Message.Contains("available") || response.Message.Contains("technician"));
    }

    [TestMethod]
    public void Process_TreatmentRecommendation_UsesCorrectArm()
    {
        var response = _agent.Process("recommend treatment for pests");
        Assert.AreEqual("TreatmentRecommendation", response.Arm);
        Assert.IsTrue(response.Message.Contains("treatment") || response.Message.Contains("Treatment"));
    }

    [TestMethod]
    public void Process_BookingLookup_PendingBookings()
    {
        var response = _agent.Process("show pending bookings");
        Assert.AreEqual("BookingLookup", response.Arm);
    }

    [TestMethod]
    public void Process_BookingLookup_UpcomingBookings()
    {
        var response = _agent.Process("show upcoming appointments");
        Assert.AreEqual("BookingLookup", response.Arm);
    }

    [TestMethod]
    public void Process_PestInfo_UsesCorrectArm()
    {
        var response = _agent.Process("tell me about rodents");
        Assert.AreEqual("PestInfo", response.Arm);
    }

    [TestMethod]
    public void Process_ReportSummary_UsesCorrectArm()
    {
        var response = _agent.Process("show inspection reports");
        Assert.AreEqual("ReportSummary", response.Arm);
    }

    [TestMethod]
    public void Process_FollowUpReports_FiltersCorrectly()
    {
        var response = _agent.Process("which reports need follow-up?");
        Assert.AreEqual("ReportSummary", response.Arm);
        Assert.IsTrue(response.Message.Contains("follow-up") || response.Message.Contains("report"));
    }

    [TestMethod]
    public void Process_DashboardStats_UsesCorrectArm()
    {
        var response = _agent.Process("how many bookings do we have?");
        Assert.AreEqual("DashboardStats", response.Arm);
        Assert.IsTrue(response.Message.Contains("Bookings") || response.Message.Contains("booking"));
    }

    [TestMethod]
    public void Process_SystemOverview_ReturnsStats()
    {
        var response = _agent.Process("show me the dashboard stats");
        Assert.AreEqual("DashboardStats", response.Arm);
        Assert.IsTrue(response.Message.Contains("Customers"));
        Assert.IsTrue(response.Message.Contains("Technicians"));
    }

    [TestMethod]
    public void Process_UnknownMessage_ReturnsFallback()
    {
        var response = _agent.Process("xyzzy gibberish nothing");
        Assert.AreEqual("general", response.Arm);
        Assert.IsTrue(response.Message.Contains("not sure"));
    }

    [TestMethod]
    public void EachArm_HasNameAndDescription()
    {
        foreach (var arm in _agent.GetArms())
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(arm.Name), "Arm name should not be empty");
            Assert.IsFalse(string.IsNullOrWhiteSpace(arm.Description), "Arm description should not be empty");
            Assert.IsTrue(arm.TriggerKeywords.Length > 0, "Arm should have at least one trigger keyword");
        }
    }
}
