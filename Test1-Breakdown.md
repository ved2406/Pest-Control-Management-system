# Test1.cs — full source and breakdown

This document contains the complete `Test1.cs` unit test file and explains what each part does.

---

## 1. Using directives (lines 1–6)

| Directive | Purpose |
|-----------|---------|
| `PestControl.Api.DataStructures` | Types such as `BinarySearchTree` used in tests. |
| `PestControl.Api.Models` | Domain models (`Customer`, `Booking`, etc.). |
| `PestControl.Api.Repositories.Static` | In-memory static repository implementations under test. |
| `PestControl.Api.Repositories.Interfaces` | Repository contracts (where referenced). |
| `PestControl.Api.Services` | `SearchService` and related services. |

These imports wire the test assembly to the API layer so tests can construct repositories and services without a database.

---

## 2. Namespace (line 7)

`namespace PestControl.Tests;` — Groups all test classes in this file into the test project’s namespace so the MSTest runner and tooling resolve types correctly.

---

## 3. MSTest attributes (used throughout)

| Attribute | Role |
|-----------|------|
| `[TestClass]` | Marks a class as a container of tests (required for discovery). |
| `[TestMethod]` | Marks an individual test method. |
| `[TestInitialize]` | Runs before each test in that class (typical use: fresh repository or service instance). |

---

## 4. `CustomerRepositoryTests` (lines 9–82)

**Purpose:** Verifies `StaticCustomerRepository` behaviour.

| Test | What it checks |
|------|----------------|
| `Setup` | Creates a new `StaticCustomerRepository` before every test so tests do not share mutated state incorrectly. |
| `GetAll_ReturnsAllCustomers` | `GetAll()` returns at least the seeded minimum (10+ customers). |
| `GetById_ValidId_ReturnsCustomer` | Id `1` exists and has expected name `"John Smith"`. |
| `GetById_InvalidId_ReturnsNull` | Unknown ids return `null`. |
| `Add_IncreasesCount` | Adding a `Customer` increases total count by one. |
| `Update_ChangesName` | `Update` persists changes (name round-trip for id 1). |
| `Delete_RemovesCustomer` | `Delete(1)` reduces count and `GetById(1)` is `null`. |
| `Search_ByName_ReturnsMatch` | Case-insensitive or partial name search finds `"John"`. |
| `Search_NoMatch_ReturnsEmpty` | Nonsense query returns an empty list. |

---

## 5. `BookingRepositoryTests` (lines 84–131)

**Purpose:** Verifies `StaticBookingRepository`.

| Test | What it checks |
|------|----------------|
| `Setup` | Fresh `StaticBookingRepository` per test. |
| `GetAll_ReturnsBookings` | Seeded data has at least 12 bookings. |
| `GetById_ValidId_ReturnsBooking` | Booking `1` exists and ties to `CustomerId == 1`. |
| `Add_CreatesBooking` | New `Booking` increases list size. |
| `Delete_RemovesBooking` | Deleting id `1` removes it. |
| `Search_ByStatus_ReturnsMatches` | Searching `"Completed"` returns results. |

---

## 6. `PestTypeRepositoryTests` (lines 133–170)

**Purpose:** Verifies `StaticPestTypeRepository`.

| Test | What it checks |
|------|----------------|
| `GetAll_ReturnsPestTypes` | At least 12 pest types in seed data. |
| `GetById_ReturnsCorrectPest` | Id `1` is `"Brown Rat"`. |
| `Search_ByCategory_ReturnsMatches` | `"Rodents"` category returns multiple items. |
| `Search_ByRiskLevel_ReturnsMatches` | `"High"` risk returns at least one match. |

---

## 7. `TechnicianRepositoryTests` (lines 172–195)

**Purpose:** Verifies `StaticTechnicianRepository`.

| Test | What it checks |
|------|----------------|
| `GetAll_ReturnsTechnicians` | Exactly four technicians (fixed seed). |
| `Search_BySpecialisation_ReturnsMatch` | `"Rodents"` specialisation returns hits. |

---

## 8. `TreatmentRepositoryTests` (lines 197–220)

**Purpose:** Verifies `StaticTreatmentRepository`.

| Test | What it checks |
|------|----------------|
| `GetAll_ReturnsTreatments` | At least 10 treatments. |
| `Search_ByMethod_ReturnsMatch` | `"Bait"` appears in at least one treatment. |

---

## 9. `InspectionReportRepositoryTests` (lines 222–253)

**Purpose:** Verifies `StaticInspectionReportRepository`.

| Test | What it checks |
|------|----------------|
| `GetAll_ReturnsReports` | At least three reports. |
| `GetByBookingId_ReturnsCorrectReport` | Booking `9` has findings mentioning `"Mouse"`. |
| `Search_ByFindings_ReturnsMatch` | `"rat"` finds at least one report. |

---

## 10. `SearchServiceTests` (lines 255–295)

**Purpose:** End-to-end checks for `SearchService` with all static repositories injected in `Setup`.

| Test | What it checks |
|------|----------------|
| `Search_EmptyQuery_ReturnsEmpty` | Empty string returns no results (guard behaviour). |
| `Search_Rat_ReturnsMultipleCategories` | Query `"rat"` aggregates matches from more than one category (customers, pests, reports, etc.). |
| `Search_CustomerName_ReturnsCustomerResult` | `"John Smith"` yields at least one result with category `"Customer"`. |

---

## 11. `BinarySearchTreeTests` (lines 297–462)

**Purpose:** Exercises `BinarySearchTree<string>` — insert, search, ordering, delete cases, helpers.

| Test | What it checks |
|------|----------------|
| `Insert_And_Search_ReturnsValue` | Basic insert and lookup by integer key. |
| `Search_NonExistentKey_ReturnsNull` | Missing key returns `null`. |
| `Insert_MultipleItems_AllRetrievable` | Several inserts; each key returns the correct value. |
| `GetAll_ReturnsSortedByKey` | `GetAll()` returns values in ascending key order. |
| `Delete_LeafNode_RemovesIt` | Deleting a leaf updates `Count` and search. |
| `Delete_NodeWithOneChild_RemovesCorrectly` | One-child delete preserves the child subtree. |
| `Delete_NodeWithTwoChildren_RemovesCorrectly` | Two-child delete; remaining keys still work; count correct. |
| `Delete_RootNode_TreeStillWorks` | Deleting the root leaves remaining structure searchable. |
| `Count_ReturnsCorrectCount` | Count tracks inserts and deletes. |
| `MaxKey_ReturnsLargestKey` | `MaxKey()` matches the largest inserted key. |
| `Filter_ReturnsMatchingValues` | `Filter` with a predicate (e.g. `StartsWith("ap")`) returns expected values. |
| `Insert_DuplicateKey_UpdatesValue` | Second insert with same key updates value without growing count. |
| `Delete_NonExistentKey_DoesNothing` | Deleting missing key does not change count. |

---

## 12. `PestControlAgentTests` (lines 464–608)

**Purpose:** Tests `PestControlAgent` — multi-“arm” routing of user messages to behaviours and response shape.

**Setup:** Builds `PestControlAgent` with the same static repositories as production-style wiring.

| Test | What it checks |
|------|----------------|
| `GetArms_ReturnsSevenArms` | Agent exposes exactly seven specialised arms. |
| `Process_EmptyMessage_ReturnsGreeting` | Empty input → `general` arm and greeting text. |
| `Process_HelloGreeting_ReturnsCapabilities` | `"hello"` → `general` and capability hints. |
| `Process_FindCustomer_UsesCustomerArm` | `"find customer John"` → `CustomerSearch` arm; message mentions customer/John. |
| `Process_CustomerSearch_AllCustomers` | Phrase like “show me all customers” → `CustomerSearch`. |
| `Process_TechnicianAvailability_UsesCorrectArm` | Availability phrasing → `TechnicianAvailability`. |
| `Process_TreatmentRecommendation_UsesCorrectArm` | Treatment recommendation → `TreatmentRecommendation`. |
| `Process_BookingLookup_PendingBookings` | Pending bookings → `BookingLookup`. |
| `Process_BookingLookup_UpcomingBookings` | Upcoming appointments → `BookingLookup`. |
| `Process_PestInfo_UsesCorrectArm` | Rodent info → `PestInfo`. |
| `Process_ReportSummary_UsesCorrectArm` | Inspection reports → `ReportSummary`. |
| `Process_FollowUpReports_FiltersCorrectly` | Follow-up question → `ReportSummary` and follow-up/report in message. |
| `Process_DashboardStats_UsesCorrectArm` | Booking count question → `DashboardStats` with booking-related text. |
| `Process_SystemOverview_ReturnsStats` | Dashboard stats phrase → `DashboardStats` with Customers/Technicians. |
| `Process_UnknownMessage_ReturnsFallback` | Gibberish → `general` and a “not sure” style reply. |
| `EachArm_HasNameAndDescription` | Every arm has non-empty name, description, and at least one trigger keyword. |

---

## 13. Full source: `Test1.cs`

The following is the complete contents of `Test1.cs` as of this breakdown (609 lines).

```csharp
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
```

---

*Line numbers in section headers refer to the original `Test1.cs` in the repository; if that file changes, re-sync this document if you need exact alignment.*
