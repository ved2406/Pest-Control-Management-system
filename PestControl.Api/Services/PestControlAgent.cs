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
    // AI agent with 7 specialist arms (customers bookings technicians treatments pests reports dashboard)
    // Selects the best arm via keyword-weighted scoring fetches live DB data then sends to Claude
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

        // Scores all arms against the user message invokes the best match to fetch DB context then calls Claude
        public async Task<AgentResponse> ProcessAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return new AgentResponse("general",
                    "Hello! I'm the PestPro AI Assistant. Ask me about customers, bookings, technicians, treatments, pest types, or reports.");

            string lower = userMessage.ToLower().Trim();

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

            string response = await CallClaudeAsync(userMessage, dataContext, armName);
            return new AgentResponse(armName, response);
        }

        public List<AgentArm> GetArms() => _arms;

        // Multi-word keywords score more than single words to reduce false matches
        private int ScoreArm(AgentArm arm, string input)
        {
            int score = 0;
            foreach (var keyword in arm.TriggerKeywords)
                if (input.Contains(keyword))
                    score += keyword.Split(' ').Length;
            return score;
        }

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

    // Returned by the agent after processing a message
    // Arm identifies which specialist handled it and Message is Claude's reply
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
