# Test plan: `Test1.cs`

## Purpose

- **Goal:** Verify static repositories, `SearchService`, custom `BinarySearchTree`, and `PestControlAgent` behaviour without a live database.
- **Fixture:** Each repository test class uses `[TestInitialize]` to create a **fresh** static repository instance so tests start from known seed data.
- **Scope:** CRUD + search on static data; cross-entity search; BST operations; agent arm selection and responses (no real HTTP to Anthropic in tests—the 6-arg agent path disables API calls).

---

## `CustomerRepositoryTests` — `StaticCustomerRepository`

| Test | Plan / assertion |
|------|------------------|
| `GetAll_ReturnsAllCustomers` | Seed has at least 10 customers. |
| `GetById_ValidId_ReturnsCustomer` | Id `1` is `"John Smith"`. |
| `GetById_InvalidId_ReturnsNull` | Missing id returns null. |
| `Add_IncreasesCount` | Add increments total count. |
| `Update_ChangesName` | Persisted update on id `1`. |
| `Delete_RemovesCustomer` | Delete reduces count and id `1` gone. |
| `Search_ByName_ReturnsMatch` | Case-insensitive name search finds `"John"`. |
| `Search_NoMatch_ReturnsEmpty` | Nonsense query returns empty list. |

---

## `BookingRepositoryTests` — `StaticBookingRepository`

| Test | Plan |
|------|------|
| `GetAll_ReturnsBookings` | At least 12 bookings in seed. |
| `GetById_ValidId_ReturnsBooking` | Id `1` has `CustomerId == 1`. |
| `Add_CreatesBooking` | Count increases after add. |
| `Delete_RemovesBooking` | Id `1` removed. |
| `Search_ByStatus_ReturnsMatches` | Search `"Completed"` returns rows. |

---

## `PestTypeRepositoryTests` — `StaticPestTypeRepository`

| Test | Plan |
|------|------|
| `GetAll_ReturnsPestTypes` | ≥ 12 pest types. |
| `GetById_ReturnsCorrectPest` | Id `1` is `"Brown Rat"`. |
| `Search_ByCategory_ReturnsMatches` | Category `"Rodents"` yields ≥ 2. |
| `Search_ByRiskLevel_ReturnsMatches` | `"High"` risk matches exist. |

---

## `TechnicianRepositoryTests` — `StaticTechnicianRepository`

| Test | Plan |
|------|------|
| `GetAll_ReturnsTechnicians` | Exactly 4 technicians in seed. |
| `Search_BySpecialisation_ReturnsMatch` | `"Rodents"` specialisation matches. |

---

## `TreatmentRepositoryTests` — `StaticTreatmentRepository`

| Test | Plan |
|------|------|
| `GetAll_ReturnsTreatments` | ≥ 10 treatments. |
| `Search_ByMethod_ReturnsMatch` | `"Bait"` appears in results. |

---

## `InspectionReportRepositoryTests` — `StaticInspectionReportRepository`

| Test | Plan |
|------|------|
| `GetAll_ReturnsReports` | ≥ 3 reports. |
| `GetByBookingId_ReturnsCorrectReport` | Booking `9` report mentions `"Mouse"`. |
| `Search_ByFindings_ReturnsMatch` | `"rat"` finds reports. |

---

## `SearchServiceTests` — cross-entity `SearchService`

| Test | Plan |
|------|------|
| `Search_EmptyQuery_ReturnsEmpty` | No query → no results. |
| `Search_Rat_ReturnsMultipleCategories` | `"rat"` hits more than one result category. |
| `Search_CustomerName_ReturnsCustomerResult` | Full name resolves to `"Customer"` category. |

---

## `BinarySearchTreeTests` — custom BST (`BinarySearchTree<string>`)

| Area | Tests |
|------|--------|
| Basic | Insert/search, missing key null. |
| Ordering | `GetAll` sorted by key. |
| Delete | Leaf, one child, two children, root; count after ops. |
| Extra | `MaxKey`, `Filter` predicate, duplicate key updates value, delete missing no-op. |

---

## `PestControlAgentTests` — `PestControlAgent` (keyword arms, no API key)

| Test | Plan |
|------|------|
| `GetArms_ReturnsSevenArms` | Agent exposes 7 arms (matches current implementation). |
| `Process_*` | Each message triggers expected `Arm` name (e.g. `CustomerSearch`, `DashboardStats`). |
| Greeting / unknown | Empty → greeting; gibberish → `"general"` fallback. |
| `EachArm_HasNameAndDescription` | Every arm has name, description, ≥1 keyword. |

---

## File header (optional comment for `Test1.cs`)

```csharp
/*
 * Test plan (Test1.cs)
 * - Static repositories: CRUD + search against seeded in-memory data.
 * - SearchService: empty query; multi-entity "rat"; customer name routing.
 * - BinarySearchTree: insert/search/delete variants, MaxKey, Filter, duplicates.
 * - PestControlAgent: arm count, Process() routing per keyword, no Claude key in tests.
 */
```
