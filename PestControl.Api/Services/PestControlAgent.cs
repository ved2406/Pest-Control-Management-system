using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PestControl.Api.Services
{
    /// <summary>
    /// AI Agent with multiple "arms" (capabilities).
    /// Each arm can READ data or WRITE data (create/delete) via SQL repositories.
    /// The agent uses keyword-weighted intent matching to select the best arm,
    /// gathers context, calls the Claude API, and executes actions when needed.
    ///
    /// Architecture:
    ///   1. User message arrives
    ///   2. Keyword-weighted intent matching selects the best arm
    ///   3. If the arm is an ACTION arm, Claude extracts structured data as JSON
    ///   4. The agent parses the JSON and executes the repository write
    ///   5. Claude generates a natural confirmation or data response
    ///
    /// Algorithm: keyword-weighted intent matching
    ///   Time complexity: O(A * K * W) where A = arms, K = keywords per arm, W = words in input
    /// </summary>
    public class PestControlAgent
    {
        private readonly List<AgentArm> _arms = new List<AgentArm>();
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private readonly ICustomerRepository _customers;
        private readonly IPestTypeRepository _pestTypes;
        private readonly IBookingRepository _bookings;
        private readonly ITechnicianRepository _technicians;
        private readonly ITreatmentRepository _treatments;
        private readonly IInspectionReportRepository _reports;

        public PestControlAgent(
            ICustomerRepository customers,
            IPestTypeRepository pestTypes,
            IBookingRepository bookings,
            ITechnicianRepository technicians,
            ITreatmentRepository treatments,
            IInspectionReportRepository reports,
            string apiKey)
        {
            _customers = customers;
            _pestTypes = pestTypes;
            _bookings = bookings;
            _technicians = technicians;
            _treatments = treatments;
            _reports = reports;
            _apiKey = apiKey;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            RegisterArms();
        }

        private void RegisterArms()
        {
            // === ACTION ARMS (create/write) — registered first so they get priority ===

            // Arm: Create booking
            _arms.Add(new AgentArm(
                "CreateBooking",
                "Create a new pest control booking",
                new[] { "create booking", "new booking", "book appointment", "book a", "schedule appointment", "make booking", "add booking", "create a booking", "assign technician", "book for" },
                input => GatherCreateBookingContext(input)
            ));

            // Arm: Add customer
            _arms.Add(new AgentArm(
                "AddCustomer",
                "Add a new customer to the system",
                new[] { "add customer", "new customer", "create customer", "register customer", "add client", "new client" },
                input => GatherAddCustomerContext(input)
            ));

            // Arm: Add technician
            _arms.Add(new AgentArm(
                "AddTechnician",
                "Add a new technician to the system",
                new[] { "add technician", "new technician", "create technician", "hire technician", "register technician" },
                input => GatherAddTechnicianContext(input)
            ));

            // Arm: Add pest type
            _arms.Add(new AgentArm(
                "AddPestType",
                "Add a new pest type to the system",
                new[] { "add pest", "new pest", "create pest", "register pest", "add pest type", "new pest type" },
                input => GatherAddPestContext(input)
            ));

            // Arm: Add treatment
            _arms.Add(new AgentArm(
                "AddTreatment",
                "Add a new treatment to the system",
                new[] { "add treatment", "new treatment", "create treatment", "register treatment" },
                input => GatherAddTreatmentContext(input)
            ));

            // === READ ARMS ===

            _arms.Add(new AgentArm(
                "CustomerSearch",
                "Search for customers by name, email, phone or address",
                new[] { "customer", "find customer", "search customer", "client", "lookup customer", "person" },
                input => GatherCustomerContext(input)
            ));

            _arms.Add(new AgentArm(
                "TechnicianAvailability",
                "Check which technicians are available or find a specific technician",
                new[] { "technician", "available", "availability", "engineer", "who is free", "who is available", "assign", "specialist", "who can" },
                input => GatherTechnicianContext(input)
            ));

            _arms.Add(new AgentArm(
                "TreatmentRecommendation",
                "Recommend treatments for a specific pest type",
                new[] { "treatment", "recommend", "product", "spray", "bait", "chemical", "solution", "how to treat", "get rid" },
                input => GatherTreatmentContext(input)
            ));

            _arms.Add(new AgentArm(
                "BookingLookup",
                "Look up bookings by status, date, or customer",
                new[] { "booking", "appointment", "schedule", "booked", "upcoming", "pending", "confirmed", "cancel" },
                input => GatherBookingContext(input)
            ));

            _arms.Add(new AgentArm(
                "PestInfo",
                "Get information about pest types, risk levels, and categories",
                new[] { "pest", "bug", "insect", "rodent", "rat", "mouse", "cockroach", "ant", "wasp", "termite", "risk", "infestation" },
                input => GatherPestContext(input)
            ));

            _arms.Add(new AgentArm(
                "ReportSummary",
                "Summarise inspection reports and follow-ups needed",
                new[] { "report", "inspection", "findings", "follow-up", "followup", "recommendations" },
                input => GatherReportContext(input)
            ));

            _arms.Add(new AgentArm(
                "DashboardStats",
                "Provide system statistics and overview",
                new[] { "stats", "statistics", "dashboard", "overview", "how many", "total", "count", "summary" },
                input => GatherDashboardContext(input)
            ));
        }

        /// <summary>
        /// Processes a user message: selects the best arm, gathers context, calls Claude API.
        /// For action arms, Claude returns JSON which is parsed and executed against SQL.
        /// </summary>
        public async Task<AgentResponse> ProcessAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return new AgentResponse("general",
                    "Hello! I'm the PestPro AI Assistant. Ask me about customers, bookings, technicians, treatments, pest types, or reports. I can also create new records!");
            }

            string lower = userMessage.ToLower().Trim();

            // Score each arm by keyword matches
            AgentArm bestArm = null;
            int bestScore = 0;

            foreach (var arm in _arms)
            {
                int score = ScoreArm(arm, lower);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestArm = arm;
                }
            }

            string dataContext;
            string armName;

            if (bestArm != null && bestScore > 0)
            {
                dataContext = bestArm.Execute(lower);
                armName = bestArm.Name;
            }
            else
            {
                dataContext = GatherDashboardContext(lower);
                armName = "general";
            }

            // Check if this is an action arm
            bool isAction = IsActionArm(armName);

            if (isAction)
            {
                // Ask Claude to extract structured JSON for the action
                string jsonResponse = await CallClaudeForActionAsync(userMessage, dataContext, armName);
                string actionResult = ExecuteAction(armName, jsonResponse);
                return new AgentResponse(armName, actionResult);
            }
            else
            {
                string response = await CallClaudeAsync(userMessage, dataContext, armName);
                return new AgentResponse(armName, response);
            }
        }

        public List<AgentArm> GetArms()
        {
            return _arms;
        }

        private bool IsActionArm(string armName)
        {
            return armName == "CreateBooking" || armName == "AddCustomer" ||
                   armName == "AddTechnician" || armName == "AddPestType" ||
                   armName == "AddTreatment";
        }

        private int ScoreArm(AgentArm arm, string input)
        {
            int score = 0;
            foreach (var keyword in arm.TriggerKeywords)
            {
                if (input.Contains(keyword))
                {
                    score += keyword.Split(' ').Length;
                }
            }
            return score;
        }

        // ========== CLAUDE API CALLS ==========

        private async Task<string> CallClaudeAsync(string userMessage, string dataContext, string armName)
        {
            try
            {
                var requestBody = new
                {
                    model = "claude-haiku-4-5-20251001",
                    max_tokens = 512,
                    system = "You are PestPro AI Assistant, a helpful chatbot embedded in a pest control management system. " +
                             "You help staff look up customers, bookings, technicians, treatments, pest info, and reports. " +
                             "You can also CREATE new bookings, customers, technicians, pest types, and treatments. " +
                             "Be concise, friendly, and professional. Use the DATA below to answer accurately. " +
                             "Do not make up data — only use what is provided. If the data is empty, say so. " +
                             "Format responses in short readable lines, not huge paragraphs. " +
                             "Keep responses under 200 words.\n\n" +
                             "ACTIVE ARM: " + armName + "\n\n" +
                             "DATA FROM SYSTEM:\n" + dataContext,
                    messages = new[]
                    {
                        new { role = "user", content = userMessage }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return "Sorry, I couldn't process that right now. Please try again.";

                using var doc = JsonDocument.Parse(responseBody);
                var textContent = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString();

                return textContent ?? "I received an empty response. Please try again.";
            }
            catch (Exception)
            {
                return "Sorry, I'm having trouble connecting right now. Please try again in a moment.";
            }
        }

        /// <summary>
        /// Calls Claude to extract structured JSON from the user's natural language request.
        /// Claude returns ONLY a JSON object with the fields needed for the action.
        /// </summary>
        private async Task<string> CallClaudeForActionAsync(string userMessage, string dataContext, string armName)
        {
            string jsonTemplate = GetJsonTemplate(armName);

            try
            {
                var requestBody = new
                {
                    model = "claude-haiku-4-5-20251001",
                    max_tokens = 512,
                    system = "You are a data extraction assistant for a pest control management system. " +
                             "Extract the relevant fields from the user's message and return ONLY valid JSON. " +
                             "No explanation, no markdown, no code fences — just the raw JSON object. " +
                             "If the user didn't specify a field, use a sensible default. " +
                             "Use the EXISTING DATA to match names to IDs where needed.\n\n" +
                             "EXISTING DATA:\n" + dataContext + "\n\n" +
                             "REQUIRED JSON FORMAT:\n" + jsonTemplate,
                    messages = new[]
                    {
                        new { role = "user", content = userMessage }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return "";

                using var doc = JsonDocument.Parse(responseBody);
                var textContent = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString();

                return textContent ?? "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private string GetJsonTemplate(string armName)
        {
            switch (armName)
            {
                case "CreateBooking":
                    return "{\"customerId\": 1, \"pestTypeId\": 1, \"technicianId\": 1, \"date\": \"2025-04-01\", \"time\": \"09:00\", \"status\": \"Pending\", \"location\": \"123 Main St\", \"notes\": \"Initial inspection\"}";
                case "AddCustomer":
                    return "{\"name\": \"John Smith\", \"address\": \"123 Main St\", \"phone\": \"07700900000\", \"email\": \"john@example.com\", \"propertyType\": \"Residential\"}";
                case "AddTechnician":
                    return "{\"name\": \"Jane Doe\", \"specialisation\": \"Rodent Control\", \"phone\": \"07700900000\", \"email\": \"jane@pestpro.com\", \"available\": true}";
                case "AddPestType":
                    return "{\"name\": \"Bedbugs\", \"category\": \"Insects\", \"description\": \"Small parasitic insects that feed on blood\", \"riskLevel\": \"Medium\"}";
                case "AddTreatment":
                    return "{\"productName\": \"RatAway Pro\", \"method\": \"Bait stations\", \"targetPestTypeId\": 1, \"safetyInfo\": \"Keep away from children\"}";
                default:
                    return "{}";
            }
        }

        // ========== ACTION EXECUTION ==========

        /// <summary>
        /// Parses the JSON returned by Claude and executes the corresponding repository write.
        /// Returns a confirmation message to the user.
        /// </summary>
        private string ExecuteAction(string armName, string jsonResponse)
        {
            if (string.IsNullOrWhiteSpace(jsonResponse))
                return "Sorry, I couldn't understand the details. Could you try again with more information?";

            try
            {
                // Strip markdown code fences if Claude added them
                jsonResponse = jsonResponse.Trim();
                if (jsonResponse.StartsWith("```"))
                {
                    jsonResponse = jsonResponse.Substring(jsonResponse.IndexOf('\n') + 1);
                    if (jsonResponse.EndsWith("```"))
                        jsonResponse = jsonResponse.Substring(0, jsonResponse.LastIndexOf("```"));
                    jsonResponse = jsonResponse.Trim();
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                switch (armName)
                {
                    case "CreateBooking":
                        return ExecuteCreateBooking(jsonResponse, options);
                    case "AddCustomer":
                        return ExecuteAddCustomer(jsonResponse, options);
                    case "AddTechnician":
                        return ExecuteAddTechnician(jsonResponse, options);
                    case "AddPestType":
                        return ExecuteAddPest(jsonResponse, options);
                    case "AddTreatment":
                        return ExecuteAddTreatment(jsonResponse, options);
                    default:
                        return "Unknown action.";
                }
            }
            catch (Exception ex)
            {
                return "Sorry, I had trouble creating that record. Error: " + ex.Message + "\nPlease try again with clearer details.";
            }
        }

        private string ExecuteCreateBooking(string json, JsonSerializerOptions opts)
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;

            var booking = new Booking(
                0,
                r.GetProperty("customerId").GetInt32(),
                r.GetProperty("pestTypeId").GetInt32(),
                r.GetProperty("technicianId").GetInt32(),
                r.GetProperty("date").GetString() ?? "2025-04-01",
                r.GetProperty("time").GetString() ?? "09:00",
                r.GetProperty("status").GetString() ?? "Pending",
                r.GetProperty("location").GetString() ?? "",
                r.GetProperty("notes").GetString() ?? ""
            );

            _bookings.Add(booking);

            var cust = _customers.GetAll().FirstOrDefault(c => c.Id == booking.CustomerId);
            var pest = _pestTypes.GetAll().FirstOrDefault(p => p.Id == booking.PestTypeId);
            var tech = _technicians.GetAll().FirstOrDefault(t => t.Id == booking.TechnicianId);

            return "Booking created successfully!\n" +
                   "Customer: " + (cust?.Name ?? "ID " + booking.CustomerId) + "\n" +
                   "Pest: " + (pest?.Name ?? "ID " + booking.PestTypeId) + "\n" +
                   "Technician: " + (tech?.Name ?? "ID " + booking.TechnicianId) + "\n" +
                   "Date: " + booking.Date + " at " + booking.Time + "\n" +
                   "Location: " + booking.Location + "\n" +
                   "Status: " + booking.Status;
        }

        private string ExecuteAddCustomer(string json, JsonSerializerOptions opts)
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;

            var customer = new Customer(
                0,
                r.GetProperty("name").GetString() ?? "New Customer",
                r.GetProperty("address").GetString() ?? "",
                r.GetProperty("phone").GetString() ?? "",
                r.GetProperty("email").GetString() ?? "",
                r.GetProperty("propertyType").GetString() ?? "Residential"
            );

            _customers.Add(customer);

            return "Customer added successfully!\n" +
                   "Name: " + customer.Name + "\n" +
                   "Address: " + customer.Address + "\n" +
                   "Phone: " + customer.Phone + "\n" +
                   "Email: " + customer.Email + "\n" +
                   "Property: " + customer.PropertyType;
        }

        private string ExecuteAddTechnician(string json, JsonSerializerOptions opts)
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;

            var tech = new Technician(
                0,
                r.GetProperty("name").GetString() ?? "New Technician",
                r.GetProperty("specialisation").GetString() ?? "General",
                r.GetProperty("phone").GetString() ?? "",
                r.GetProperty("email").GetString() ?? "",
                r.TryGetProperty("available", out var avail) ? avail.GetBoolean() : true
            );

            _technicians.Add(tech);

            return "Technician added successfully!\n" +
                   "Name: " + tech.Name + "\n" +
                   "Specialisation: " + tech.Specialisation + "\n" +
                   "Phone: " + tech.Phone + "\n" +
                   "Email: " + tech.Email + "\n" +
                   "Available: " + (tech.Available ? "Yes" : "No");
        }

        private string ExecuteAddPest(string json, JsonSerializerOptions opts)
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;

            var pest = new PestType(
                0,
                r.GetProperty("name").GetString() ?? "New Pest",
                r.GetProperty("category").GetString() ?? "Other",
                r.GetProperty("description").GetString() ?? "",
                r.GetProperty("riskLevel").GetString() ?? "Medium"
            );

            _pestTypes.Add(pest);

            return "Pest type added successfully!\n" +
                   "Name: " + pest.Name + "\n" +
                   "Category: " + pest.Category + "\n" +
                   "Risk Level: " + pest.RiskLevel + "\n" +
                   "Description: " + pest.Description;
        }

        private string ExecuteAddTreatment(string json, JsonSerializerOptions opts)
        {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;

            var treatment = new Treatment(
                0,
                r.GetProperty("productName").GetString() ?? "New Treatment",
                r.GetProperty("method").GetString() ?? "",
                r.GetProperty("targetPestTypeId").GetInt32(),
                r.GetProperty("safetyInfo").GetString() ?? ""
            );

            _treatments.Add(treatment);

            var pest = _pestTypes.GetAll().FirstOrDefault(p => p.Id == treatment.TargetPestTypeId);

            return "Treatment added successfully!\n" +
                   "Product: " + treatment.ProductName + "\n" +
                   "Method: " + treatment.Method + "\n" +
                   "Target Pest: " + (pest?.Name ?? "ID " + treatment.TargetPestTypeId) + "\n" +
                   "Safety Info: " + treatment.SafetyInfo;
        }

        // ========== CONTEXT GATHERING METHODS ==========

        private string GatherCreateBookingContext(string input)
        {
            var sb = new StringBuilder();
            sb.AppendLine("CUSTOMERS:");
            foreach (var c in _customers.GetAll())
                sb.AppendLine("- ID:" + c.Id + " " + c.Name);
            sb.AppendLine("\nPEST TYPES:");
            foreach (var p in _pestTypes.GetAll())
                sb.AppendLine("- ID:" + p.Id + " " + p.Name);
            sb.AppendLine("\nTECHNICIANS (available):");
            foreach (var t in _technicians.GetAll().Where(t => t.Available))
                sb.AppendLine("- ID:" + t.Id + " " + t.Name + " (" + t.Specialisation + ")");
            return sb.ToString();
        }

        private string GatherAddCustomerContext(string input)
        {
            return "PropertyType options: Residential, Commercial, Industrial\nExisting customers: " + _customers.GetAll().Count;
        }

        private string GatherAddTechnicianContext(string input)
        {
            return "Common specialisations: Rodent Control, Insect Control, Bird Control, General Pest Control, Fumigation, Termite Specialist\nExisting technicians: " + _technicians.GetAll().Count;
        }

        private string GatherAddPestContext(string input)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Category options: Insects, Rodents, Birds, Wildlife, Other");
            sb.AppendLine("RiskLevel options: Low, Medium, High");
            sb.AppendLine("Existing pest types:");
            foreach (var p in _pestTypes.GetAll())
                sb.AppendLine("- " + p.Name);
            return sb.ToString();
        }

        private string GatherAddTreatmentContext(string input)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PEST TYPES (use ID for targetPestTypeId):");
            foreach (var p in _pestTypes.GetAll())
                sb.AppendLine("- ID:" + p.Id + " " + p.Name);
            return sb.ToString();
        }

        private string GatherCustomerContext(string input)
        {
            var all = _customers.GetAll();
            var sb = new StringBuilder();
            sb.AppendLine("CUSTOMERS (" + all.Count + " total):");
            foreach (var c in all)
                sb.AppendLine("- ID:" + c.Id + " | " + c.Name + " | " + c.Address + " | " + c.Phone + " | " + c.Email + " | " + c.PropertyType);
            return sb.ToString();
        }

        private string GatherTechnicianContext(string input)
        {
            var all = _technicians.GetAll();
            var sb = new StringBuilder();
            sb.AppendLine("TECHNICIANS (" + all.Count + " total):");
            foreach (var t in all)
                sb.AppendLine("- ID:" + t.Id + " | " + t.Name + " | " + t.Specialisation + " | " + t.Phone + " | " + (t.Available ? "AVAILABLE" : "UNAVAILABLE"));
            return sb.ToString();
        }

        private string GatherTreatmentContext(string input)
        {
            var allTreatments = _treatments.GetAll();
            var allPests = _pestTypes.GetAll();
            var sb = new StringBuilder();
            sb.AppendLine("TREATMENTS (" + allTreatments.Count + " total):");
            foreach (var t in allTreatments)
            {
                var pest = allPests.FirstOrDefault(p => p.Id == t.TargetPestTypeId);
                string pestName = pest != null ? pest.Name : "General";
                sb.AppendLine("- ID:" + t.Id + " | " + t.ProductName + " | Method: " + t.Method + " | For: " + pestName + " | Safety: " + t.SafetyInfo);
            }
            sb.AppendLine("\nPEST TYPES:");
            foreach (var p in allPests)
                sb.AppendLine("- " + p.Name + " (" + p.Category + ") — Risk: " + p.RiskLevel);
            return sb.ToString();
        }

        private string GatherBookingContext(string input)
        {
            var allBookings = _bookings.GetAll();
            var allCustomers = _customers.GetAll();
            var allPests = _pestTypes.GetAll();
            var sb = new StringBuilder();
            sb.AppendLine("BOOKINGS (" + allBookings.Count + " total):");
            foreach (var b in allBookings)
            {
                var cust = allCustomers.FirstOrDefault(c => c.Id == b.CustomerId);
                var pest = allPests.FirstOrDefault(p => p.Id == b.PestTypeId);
                string custName = cust != null ? cust.Name : "Unknown";
                string pestName = pest != null ? pest.Name : "Unknown";
                sb.AppendLine("- Booking #" + b.Id + " | " + b.Date + " " + b.Time + " | Customer: " + custName + " | Pest: " + pestName + " | Status: " + b.Status + " | Location: " + b.Location);
            }
            return sb.ToString();
        }

        private string GatherPestContext(string input)
        {
            var all = _pestTypes.GetAll();
            var sb = new StringBuilder();
            sb.AppendLine("PEST TYPES (" + all.Count + " total):");
            foreach (var p in all)
                sb.AppendLine("- ID:" + p.Id + " | " + p.Name + " | Category: " + p.Category + " | Risk: " + p.RiskLevel + " | " + p.Description);
            return sb.ToString();
        }

        private string GatherReportContext(string input)
        {
            var all = _reports.GetAll();
            var sb = new StringBuilder();
            sb.AppendLine("INSPECTION REPORTS (" + all.Count + " total):");
            foreach (var r in all)
            {
                sb.AppendLine("- Report #" + r.Id + " | Booking #" + r.BookingId + " | " + r.ReportDate + " | Follow-up: " + (r.FollowUpNeeded ? "YES" : "NO"));
                sb.AppendLine("  Findings: " + r.Findings);
                sb.AppendLine("  Recommendations: " + r.Recommendations);
            }
            return sb.ToString();
        }

        private string GatherDashboardContext(string input)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SYSTEM OVERVIEW:");
            sb.AppendLine("- Customers: " + _customers.GetAll().Count);
            sb.AppendLine("- Pest Types: " + _pestTypes.GetAll().Count);
            var techs = _technicians.GetAll();
            sb.AppendLine("- Technicians: " + techs.Count + " (" + techs.Count(t => t.Available) + " available)");
            var bookings = _bookings.GetAll();
            sb.AppendLine("- Bookings: " + bookings.Count);
            foreach (var g in bookings.GroupBy(b => b.Status))
                sb.AppendLine("  - " + g.Key + ": " + g.Count());
            sb.AppendLine("- Treatments: " + _treatments.GetAll().Count);
            var reports = _reports.GetAll();
            sb.AppendLine("- Inspection Reports: " + reports.Count + " (" + reports.Count(r => r.FollowUpNeeded) + " need follow-up)");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Response object returned by the agent after processing a message.
    /// </summary>
    public class AgentResponse
    {
        public string Arm { get; set; }
        public string Message { get; set; }

        public AgentResponse(string arm, string message)
        {
            Arm = arm;
            Message = message;
        }
    }
}
