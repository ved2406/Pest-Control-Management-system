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
    /// PestControlAgent is the AI brain of the system.
    /// It has 7 "arms" — each arm is a specialist in one area (customers, bookings, pests, etc).
    /// When a user types a message, the agent:
    ///   1. Reads the message and scores every arm using keyword matching
    ///   2. Picks the arm with the highest score (most relevant to the message)
    ///   3. That arm fetches real data from the SQL database
    ///   4. The data + user message are sent to Claude AI (Anthropic API)
    ///   5. Claude reads the real data and writes a helpful natural language reply
    ///
    /// Algorithm used: Keyword-Weighted Intent Matching
    ///   - Multi-word keywords score higher than single words
    ///   - e.g. "find customer" scores 2 points, "customer" scores 1 point
    ///   - Time complexity: O(A * K) where A = number of arms, K = keywords per arm
    /// </summary>
    public class PestControlAgent
    {
        // The list of all arms (capabilities) this agent has
        private readonly List<AgentArm> _arms = new List<AgentArm>();

        // HttpClient is used to make HTTP requests to the Claude API
        private readonly HttpClient _httpClient;

        // The Claude API key used to authenticate our requests to Anthropic
        private readonly string _apiKey;

        // These are the 6 repository interfaces — each one talks to a different SQL table
        private readonly ICustomerRepository _customers;
        private readonly IPestTypeRepository _pestTypes;
        private readonly IBookingRepository _bookings;
        private readonly ITechnicianRepository _technicians;
        private readonly ITreatmentRepository _treatments;
        private readonly IInspectionReportRepository _reports;

        /// <summary>
        /// Constructor — called once when the app starts (via Program.cs dependency injection).
        /// Stores all 6 repositories, sets up the Claude API connection, and registers all 7 arms.
        /// </summary>
        public PestControlAgent(
            ICustomerRepository customers,
            IPestTypeRepository pestTypes,
            IBookingRepository bookings,
            ITechnicianRepository technicians,
            ITreatmentRepository treatments,
            IInspectionReportRepository reports,
            string apiKey)
        {
            // Store each repository so arms can use them later to fetch data
            _customers = customers;
            _pestTypes = pestTypes;
            _bookings = bookings;
            _technicians = technicians;
            _treatments = treatments;
            _reports = reports;
            _apiKey = apiKey;

            // Set up the HTTP client with the Claude API authentication headers
            // Every request to Claude must include these two headers
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);           // Our API key
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01"); // API version

            // Register all 7 arms so the agent knows what it can do
            RegisterArms();
        }

        /// <summary>
        /// RegisterArms creates all 7 arms and adds them to the _arms list.
        /// Each arm has:
        ///   - A name (used to identify which arm was chosen)
        ///   - A description (explains what this arm does)
        ///   - Trigger keywords (words in the user's message that activate this arm)
        ///   - An Execute function (fetches the relevant data from the database)
        /// </summary>
        private void RegisterArms()
        {
            // ARM 1: CustomerSearch
            // Triggered when the user asks about customers, clients, or specific people
            _arms.Add(new AgentArm(
                "CustomerSearch",
                "Search for customers by name, email, phone or address",
                new[] { "customer", "find customer", "search customer", "client", "lookup customer", "person" },
                input => GatherCustomerContext(input)  // calls GatherCustomerContext to get all customer data
            ));

            // ARM 2: TechnicianAvailability
            // Triggered when the user wants to know who is available to do a job
            _arms.Add(new AgentArm(
                "TechnicianAvailability",
                "Check which technicians are available or find a specific technician",
                new[] { "technician", "available", "availability", "engineer", "who is free", "who is available", "assign", "specialist", "who can" },
                input => GatherTechnicianContext(input)
            ));

            // ARM 3: TreatmentRecommendation
            // Triggered when the user wants to know what product or method to use on a pest
            _arms.Add(new AgentArm(
                "TreatmentRecommendation",
                "Recommend treatments for a specific pest type",
                new[] { "treatment", "recommend", "product", "spray", "bait", "chemical", "solution", "how to treat", "get rid" },
                input => GatherTreatmentContext(input)
            ));

            // ARM 4: BookingLookup
            // Triggered when the user asks about appointments or job schedules
            _arms.Add(new AgentArm(
                "BookingLookup",
                "Look up bookings by status, date, or customer",
                new[] { "booking", "appointment", "schedule", "booked", "upcoming", "pending", "confirmed", "cancel" },
                input => GatherBookingContext(input)
            ));

            // ARM 5: PestInfo
            // Triggered when the user asks about a specific pest type or infestation
            _arms.Add(new AgentArm(
                "PestInfo",
                "Get information about pest types, risk levels, and categories",
                new[] { "pest", "bug", "insect", "rodent", "rat", "mouse", "cockroach", "ant", "wasp", "termite", "risk", "infestation" },
                input => GatherPestContext(input)
            ));

            // ARM 6: ReportSummary
            // Triggered when the user wants to review inspection findings or follow-up actions
            _arms.Add(new AgentArm(
                "ReportSummary",
                "Summarise inspection reports and follow-ups needed",
                new[] { "report", "inspection", "findings", "follow-up", "followup", "recommendations" },
                input => GatherReportContext(input)
            ));

            // ARM 7: DashboardStats
            // Triggered when the user wants a general overview or count of records
            _arms.Add(new AgentArm(
                "DashboardStats",
                "Provide system statistics and overview",
                new[] { "stats", "statistics", "dashboard", "overview", "how many", "total", "count", "summary" },
                input => GatherDashboardContext(input)
            ));
        }

        /// <summary>
        /// ProcessAsync is the main entry point — called every time the user sends a chat message.
        ///
        /// Step 1: Convert message to lowercase so keyword matching is case-insensitive
        /// Step 2: Score every arm using ScoreArm() — pick the one with the highest score
        /// Step 3: Run that arm's Execute() function to fetch real data from SQL
        /// Step 4: Send the data + user message to Claude API
        /// Step 5: Return Claude's response back to the chat widget
        /// </summary>
        public async Task<AgentResponse> ProcessAsync(string userMessage)
        {
            // If the message is empty, return a greeting immediately without calling Claude
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return new AgentResponse("general",
                    "Hello! I'm the PestPro AI Assistant. Ask me about customers, bookings, technicians, treatments, pest types, or reports.");
            }

            // Normalise the input — lowercase and trimmed for consistent keyword matching
            string lower = userMessage.ToLower().Trim();

            // Score every arm and find the best match
            AgentArm bestArm = null;
            int bestScore = 0;

            foreach (var arm in _arms)
            {
                int score = ScoreArm(arm, lower); // count how many of this arm's keywords appear in the message
                if (score > bestScore)
                {
                    bestScore = score;
                    bestArm = arm; // this arm is currently the best match
                }
            }

            string dataContext;
            string armName;

            if (bestArm != null && bestScore > 0)
            {
                // An arm was matched — fetch the relevant data using its Execute function
                dataContext = bestArm.Execute(lower);
                armName = bestArm.Name;
            }
            else
            {
                // No arm matched — fall back to general dashboard stats
                dataContext = GatherDashboardContext(lower);
                armName = "general";
            }

            // Send the user message + fetched data to Claude and return the AI response
            string response = await CallClaudeAsync(userMessage, dataContext, armName);
            return new AgentResponse(armName, response);
        }

        /// <summary>
        /// Returns the list of all registered arms.
        /// Used by GET /api/agent/arms to show what the agent can do.
        /// </summary>
        public List<AgentArm> GetArms()
        {
            return _arms;
        }

        /// <summary>
        /// ScoreArm calculates how relevant an arm is to the user's message.
        /// It loops through all the arm's trigger keywords and checks if the message contains them.
        /// Multi-word keywords score more points than single words.
        ///   - "find customer" = 2 points (2 words)
        ///   - "customer"      = 1 point  (1 word)
        /// This means specific phrases beat generic single words, reducing false matches.
        /// </summary>
        private int ScoreArm(AgentArm arm, string input)
        {
            int score = 0;
            foreach (var keyword in arm.TriggerKeywords)
            {
                if (input.Contains(keyword)) // check if this keyword appears anywhere in the message
                {
                    score += keyword.Split(' ').Length; // add 1 point per word in the keyword
                }
            }
            return score;
        }

        // ========== CLAUDE API CALL ==========

        /// <summary>
        /// CallClaudeAsync sends a POST request to the Anthropic Claude API.
        /// We do NOT use any SDK — we call the REST API directly using HttpClient.
        ///
        /// The request contains:
        ///   - model: which Claude model to use
        ///   - system prompt: tells Claude who it is and gives it the real SQL data as context
        ///   - user message: what the user typed in the chat
        ///
        /// Claude reads the real data we provide and generates a helpful response.
        /// We parse the JSON response and extract just the text content.
        /// </summary>
        private async Task<string> CallClaudeAsync(string userMessage, string dataContext, string armName)
        {
            try
            {
                // Build the request body as an anonymous object — will be serialised to JSON
                var requestBody = new
                {
                    model = "claude-haiku-4-5-20251001", // the Claude model we're using
                    max_tokens = 512,                    // limit response length to 512 tokens

                    // The system prompt tells Claude its role and gives it the database data
                    system = "You are PestPro AI Assistant, a helpful chatbot embedded in a pest control management system. " +
                             "You help staff look up customers, bookings, technicians, treatments, pest info, and reports. " +
                             "Be concise, friendly, and professional. Use the DATA below to answer accurately. " +
                             "Do not make up data — only use what is provided. If the data is empty, say so. " +
                             "Format responses in short readable lines, not huge paragraphs. " +
                             "Keep responses under 200 words.\n\n" +
                             "ACTIVE ARM: " + armName + "\n\n" +      // tells Claude which arm is active
                             "DATA FROM SYSTEM:\n" + dataContext,      // the real SQL data

                    // The user's actual message
                    messages = new[]
                    {
                        new { role = "user", content = userMessage }
                    }
                };

                // Serialise the object to a JSON string and wrap it in an HTTP content body
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // POST to the Anthropic API endpoint
                var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                // If Claude returned an error status code, show a friendly message
                if (!response.IsSuccessStatusCode)
                    return "Sorry, I couldn't process that right now. Please try again.";

                // Parse the JSON response and extract the text from Claude's reply
                // The response structure is: { content: [ { text: "..." } ] }
                using var doc = JsonDocument.Parse(responseBody);
                var textContent = doc.RootElement
                    .GetProperty("content")[0]   // first content block
                    .GetProperty("text")          // the text field
                    .GetString();

                return textContent ?? "I received an empty response. Please try again.";
            }
            catch (Exception)
            {
                // Network error or timeout — return a user-friendly message
                return "Sorry, I'm having trouble connecting right now. Please try again in a moment.";
            }
        }

        // ========== CONTEXT GATHERING METHODS ==========
        // These methods are called by each arm's Execute() function.
        // They query the SQL database via the repository interfaces and
        // format the data as a plain-text string that gets sent to Claude as context.

        /// <summary>
        /// Fetches all customers from the database.
        /// Builds a formatted list with ID, name, address, phone, email, and property type.
        /// This data is given to Claude so it can answer customer-related questions.
        /// </summary>
        private string GatherCustomerContext(string input)
        {
            var all = _customers.GetAll(); // SQL: SELECT * FROM Customers
            var sb = new StringBuilder();
            sb.AppendLine("CUSTOMERS (" + all.Count + " total):");
            foreach (var c in all)
                sb.AppendLine("- ID:" + c.Id + " | " + c.Name + " | " + c.Address + " | " + c.Phone + " | " + c.Email + " | " + c.PropertyType);
            return sb.ToString();
        }

        /// <summary>
        /// Fetches all technicians and their availability status.
        /// Claude uses this to answer questions like "who is free today?"
        /// </summary>
        private string GatherTechnicianContext(string input)
        {
            var all = _technicians.GetAll(); // SQL: SELECT * FROM Technicians
            var sb = new StringBuilder();
            sb.AppendLine("TECHNICIANS (" + all.Count + " total):");
            foreach (var t in all)
                sb.AppendLine("- ID:" + t.Id + " | " + t.Name + " | " + t.Specialisation + " | " + t.Phone + " | " + (t.Available ? "AVAILABLE" : "UNAVAILABLE"));
            return sb.ToString();
        }

        /// <summary>
        /// Fetches all treatments AND all pest types together.
        /// Joins them so Claude knows which treatment is for which pest.
        /// e.g. "RatAway Pro | Bait stations | For: Brown Rat"
        /// </summary>
        private string GatherTreatmentContext(string input)
        {
            var allTreatments = _treatments.GetAll();
            var allPests = _pestTypes.GetAll();
            var sb = new StringBuilder();
            sb.AppendLine("TREATMENTS (" + allTreatments.Count + " total):");
            foreach (var t in allTreatments)
            {
                // Find the pest name by matching TargetPestTypeId to the pest's Id
                var pest = allPests.FirstOrDefault(p => p.Id == t.TargetPestTypeId);
                string pestName = pest != null ? pest.Name : "General";
                sb.AppendLine("- ID:" + t.Id + " | " + t.ProductName + " | Method: " + t.Method + " | For: " + pestName + " | Safety: " + t.SafetyInfo);
            }
            sb.AppendLine("\nPEST TYPES:");
            foreach (var p in allPests)
                sb.AppendLine("- " + p.Name + " (" + p.Category + ") — Risk: " + p.RiskLevel);
            return sb.ToString();
        }

        /// <summary>
        /// Fetches all bookings and resolves customer names and pest names.
        /// Without this join, Claude would only see IDs (e.g. CustomerId=3) instead of names.
        /// </summary>
        private string GatherBookingContext(string input)
        {
            var allBookings = _bookings.GetAll();
            var allCustomers = _customers.GetAll();
            var allPests = _pestTypes.GetAll();
            var sb = new StringBuilder();
            sb.AppendLine("BOOKINGS (" + allBookings.Count + " total):");
            foreach (var b in allBookings)
            {
                // Look up the customer name and pest name using the foreign key IDs
                var cust = allCustomers.FirstOrDefault(c => c.Id == b.CustomerId);
                var pest = allPests.FirstOrDefault(p => p.Id == b.PestTypeId);
                string custName = cust != null ? cust.Name : "Unknown";
                string pestName = pest != null ? pest.Name : "Unknown";
                sb.AppendLine("- Booking #" + b.Id + " | " + b.Date + " " + b.Time + " | Customer: " + custName + " | Pest: " + pestName + " | Status: " + b.Status + " | Location: " + b.Location);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Fetches all pest types with their category, risk level, and description.
        /// Claude uses this to explain what a pest is and how dangerous it is.
        /// </summary>
        private string GatherPestContext(string input)
        {
            var all = _pestTypes.GetAll(); // SQL: SELECT * FROM PestTypes
            var sb = new StringBuilder();
            sb.AppendLine("PEST TYPES (" + all.Count + " total):");
            foreach (var p in all)
                sb.AppendLine("- ID:" + p.Id + " | " + p.Name + " | Category: " + p.Category + " | Risk: " + p.RiskLevel + " | " + p.Description);
            return sb.ToString();
        }

        /// <summary>
        /// Fetches all inspection reports with findings and recommendations.
        /// Also shows whether a follow-up visit is needed.
        /// </summary>
        private string GatherReportContext(string input)
        {
            var all = _reports.GetAll(); // SQL: SELECT * FROM InspectionReports
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

        /// <summary>
        /// Builds a system-wide summary: total counts, booking statuses, technician availability.
        /// Used as the fallback context when no arm matches, and for the DashboardStats arm.
        /// GroupBy groups bookings by status so we can count e.g. Pending=3, Completed=7.
        /// </summary>
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
            // Group bookings by their Status field and count each group
            foreach (var g in bookings.GroupBy(b => b.Status))
                sb.AppendLine("  - " + g.Key + ": " + g.Count());
            sb.AppendLine("- Treatments: " + _treatments.GetAll().Count);
            var reports = _reports.GetAll();
            sb.AppendLine("- Inspection Reports: " + reports.Count + " (" + reports.Count(r => r.FollowUpNeeded) + " need follow-up)");
            return sb.ToString();
        }
    }

    /// <summary>
    /// AgentResponse is what the agent returns after processing a message.
    /// - Arm: which arm handled the request (e.g. "CustomerSearch", "PestInfo")
    /// - Message: the text response from Claude to show in the chat
    /// The frontend uses Arm to show a label under the chat bubble.
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