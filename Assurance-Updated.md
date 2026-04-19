# Assurance Updated — Quality assurance report

**Product:** PestPro / Pest Control Management System (updated codebase)  
**Report date:** 19 April 2026  
**QA type:** Automated build, unit test execution, lightweight HTTP smoke check  

This document records the quality assurance run performed on the **current updated** version of the software (including configuration and tooling changes such as .NET 7 targeting and development static-repository mode).

---

## 1. Environment

| Item | Value |
|------|--------|
| Host OS | macOS (darwin) |
| .NET SDK | 7.x (as reported by `dotnet build` / `dotnet test`) |
| Target framework | `net7.0` (`PestControl.Api`, `PestControl.Tests`) |
| Test framework | MSTest 3.x (`Microsoft.NET.Test.Sdk`, `MSTest.TestAdapter`, `MSTest.TestFramework`) |

---

## 2. Build verification

**Command:** `dotnet build --no-incremental` in `PestControl.Api/`

| Result | Detail |
|--------|--------|
| **Outcome** | **Succeeded** — 0 errors |
| Warnings | **24** (API project), mostly nullable reference type (`CS86xx`) and a few `Program.cs` / `PestControlAgent` nullability notes |

**Interpretation:** The solution compiles cleanly for release/debug. Warnings do not block execution but indicate opportunities to tighten nullability annotations (`string?` for API keys, nullable returns on repository `GetById`, etc.) for long-term maintainability.

---

## 3. Automated test execution (unit / integration scope)

**Command:** `dotnet test PestControl.Tests/PestControl.Tests.csproj --logger "console;verbosity=normal"`

| Metric | Value |
|--------|--------|
| **Total tests** | 56 |
| **Passed** | 56 |
| **Failed** | 0 |
| **Skipped** | 0 |
| **Duration** | ~0.33 s (machine-dependent) |
| Test parallelization | Enabled (method level, 10 workers) |

**Coverage (by test suite in `Test1.cs`):**

- Static repository behaviour (customers, bookings, pest types, technicians, treatments, inspection reports): CRUD and search.
- `SearchService`: cross-entity search, empty query, multi-category matches.
- `BinarySearchTree`: insert, search, delete variants, ordering, `MaxKey`, `Filter`, duplicate keys.
- `PestControlAgent`: arm list, keyword routing, greetings/fallback (tests use static repos and **no live Anthropic API key**).

**Limitation:** Automated tests do **not** exercise SQL Server repositories, real database connectivity, or live Claude/Anthropic calls. Those require separate integration or manual QA with `Data:UseStaticRepositories` set to `false` and valid `ConnectionStrings:PestControl` and `AnthropicApiKey` / `ANTHROPIC_API_KEY`.

---

## 4. HTTP smoke check (manual/lightweight)

**Checks performed:** HTTP `GET` to application root and to `index.html` on the configured dev port (**5073** per `Properties/launchSettings.json`).

| Endpoint | HTTP status |
|----------|-------------|
| `/` | 200 |
| `/index.html` | 200 |

**Note:** During this run, port 5073 was already bound (a separate `dotnet run` instance may have been active). A second bind attempt failed with “address already in use”; smoke requests still received **200** from the listening process. For a clean smoke, ensure no duplicate server instance before starting the app.

---

## 5. Overall assessment (Assurance Updated)

| Area | Status |
|------|--------|
| Compile | Pass |
| Unit tests (`PestControl.Tests`) | **Pass — 56/56** |
| Static dev path (in-memory repositories) | Covered by tests |
| Web shell availability | Smoke: 200 on `/` and `index.html` |
| SQL-backed deployment | Not covered by this run |
| AI agent (live API) | Not covered by this run |

**Conclusion:** The **updated** software passes the executed automated QA: **build succeeds**, **all 56 tests pass**, and **basic HTTP delivery** of the static frontend responds successfully. Remaining risk sits in **database-backed** and **external API** paths, which should be validated separately when those features are required in production or coursework demos.

---

## 6. Suggested follow-up (optional)

1. Run **integration tests** or manual UI checks against SQL Server after applying `Database/CreateDatabase.sql` and disabling static repositories.
2. Configure **Anthropic API key** and spot-check the chat agent end-to-end.
3. Address **nullable reference warnings** incrementally to reduce noise and catch real null bugs earlier.
4. Add **CI** (e.g. GitHub Actions) to run `dotnet build` and `dotnet test` on each push.

---

*End of Assurance Updated report.*
