# PestPro - Pest Control Management Booking System

A full-stack pest control management system built with .NET Core Web API and a vanilla HTML/CSS/JS frontend.

## Features

- **Dashboard** — Real-time overview of bookings, technicians, customers, and reports
- **Booking Management** — Create, view, and delete pest control appointments
- **Customer Management** — Add, view, and manage customer records
- **Technician Management** — Track technician availability and specialisations
- **Pest Type Database** — Categorised pest types with risk levels
- **Treatment Catalogue** — Treatments linked to specific pest types with safety info
- **Inspection Reports** — Track findings, recommendations, and follow-ups
- **Cross-Entity Search** — Search across all data with clickable results
- **AI Agent** — Claude-powered chatbot with 7 read capabilities that can query data via natural language

## Tech Stack

- **Backend:** .NET Core Web API (net10.0)
- **Database:** SQL Server (Microsoft.Data.SqlClient, no Entity Framework)
- **Frontend:** Vanilla HTML, CSS, JavaScript (no third-party libraries)
- **Data Structures:** Custom Binary Search Tree (BST) — no STL
- **Testing:** MSTest
- **AI Integration:** Claude API (Anthropic)

## Architecture

- **Repository Pattern** — Interfaces with SQL and Static implementations
- **Custom BST** — Generic binary search tree for in-memory data operations (O(log n) search/insert)
- **AI Agent with Arms** — Keyword-weighted intent matching selects the best arm, gathers SQL data as context, and calls Claude API for natural language responses
- **SPA Frontend** — Single-page app with dynamic page loading and modal overlays

## Project Structure

```
PestControl.sln
PestControl.Api/
  Controllers/         — API controllers (Customers, Bookings, Technicians, etc.)
  Models/              — Data models (Customer, Booking, PestType, etc.)
  DataStructures/      — Custom BST implementation
  Repositories/
    Interfaces/        — Repository interfaces
    Sql/               — SQL Server implementations
    Static/            — In-memory static implementations
  Services/
    SearchService.cs   — Cross-entity search
    PestControlAgent.cs — AI Agent with 12 arms
    AgentArm.cs        — Agent arm/capability definition
  wwwroot/             — Frontend (HTML, CSS, JS)
  Database/            — SQL creation scripts
PestControl.Tests/     — MSTest unit tests
```

## Setup

### Prerequisites
- .NET 10 SDK
- SQL Server (Express or full) with SSMS
- Claude API key (for AI Agent)

### Database Setup
1. Open SSMS and connect to your SQL Server instance
2. Run the script in `PestControl.Api/Database/CreateDatabase.sql`

### API Key Setup (AI Agent)
The AI Agent requires an Anthropic API key. This is never committed to the repo.

1. Copy the template file:
   ```
   PestControl.Api/appsettings.Development.template.json → PestControl.Api/appsettings.Development.json
   ```
2. Open `appsettings.Development.json` and replace `INSERT_API_KEY_HERE` with your actual Anthropic API key
3. The file is gitignored so your key stays local

Alternatively, set an environment variable:
```bash
ANTHROPIC_API_KEY=your_key_here
```

### Running the App
```bash
cd PestControl.Api
dotnet run
```
Open `http://localhost:5073` in your browser.

### Running Tests
```bash
cd PestControl.Tests
dotnet test
```

## AI Agent

The AI Agent is a chatbot accessible via the floating chat button. It uses 7 specialised "read arms" for querying data:

**Read Arms:** Customer Search, Technician Availability, Treatment Recommendation, Booking Lookup, Pest Info, Report Summary, Dashboard Stats

Example queries:
- "show me all customers"
- "who is available?"
- "recommend treatment for rats"
- "find technician specialising in rodents"
- "what bookings do we have tomorrow?"

## CST2550 Group Coursework

Built as part of the CST2550 module. Implements all required components: custom data structures, SQL database, unit tests, search functionality, and AI agent with arms for extra marks.
