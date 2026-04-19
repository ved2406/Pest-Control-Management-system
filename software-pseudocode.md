# Pest Control Management System — simple pseudocode

High-level pseudocode for how the software fits together: ASP.NET Core API, SQL repositories, static `wwwroot` UI, and agent/search services.

---

## 1. Application startup

```
START APP
  READ connection string and optional Anthropic API key
  REGISTER singletons: Sql*Repository for each entity (Customer, PestType, Booking, ...)
  REGISTER SearchService
  REGISTER PestControlAgent (repositories + API key)
  ENABLE static files + default page + map controllers
  LISTEN for HTTP requests
END
```

---

## 2. Typical REST request (any controller)

```
ON HTTP request (GET/POST/PUT/DELETE) to /api/{resource}
  ROUTE to matching Controller action
  ACTION calls injected I*Repository method
  REPOSITORY runs SQL against PestControlDB (CRUD or Search)
  RETURN JSON to client
END
```

---

## 3. Global search (`SearchService`)

```
FUNCTION Search(query)
  IF query is empty THEN return empty list

  results = []
  APPEND matches from customers.Search(query)   AS SearchResult rows
  APPEND matches from pestTypes.Search(query)
  APPEND matches from bookings.Search(query)
  APPEND matches from technicians.Search(query)
  APPEND matches from treatments.Search(query)
  APPEND matches from inspectionReports.Search(query)

  RETURN combined results
END
```

---

## 4. AI agent (`PestControlAgent`)

```
FUNCTION ProcessAsync(userMessage)
  IF message is empty THEN return greeting / help (no arm)

  FOR each registered "arm" (CustomerSearch, TechnicianAvailability, ...)
    SCORE message against arm’s trigger keywords
  PICK arm with best score (or fallback to "general")

  context = arm’s Gather*Context(userMessage)   // loads live rows from repositories

  IF API key missing THEN return structured response without Claude (tests / offline)
  ELSE
    BUILD prompt with user message + context
    CALL Anthropic API
    RETURN AgentResponse(arm name, assistant text)
  END IF
END
```

*(Unit tests use a synchronous `Process` path with static repositories and no API key.)*

---

## 5. Binary search tree (data structure module)

```
FUNCTION Insert(key, value)
  PLACE node in BST by key OR update value if key exists
END

FUNCTION Search(key)
  WALK tree from root following left/right by key
  RETURN value or null
END

FUNCTION Delete(key)
  REMOVE node handling leaf / one child / two children / root cases
END

FUNCTION GetAll
  IN-ORDER traversal → values sorted by key
END
```

---

## 6. Browser UI (`wwwroot` JS)

```
ON page load
  WIRE navigation, modal, global search box
  LOAD default page (e.g. dashboard)

FUNCTION loadPage(pageId)
  FETCH JSON from /api/* as needed
  RENDER lists, charts, tables in DOM
END

ON global search Enter
  NAVIGATE to search view
  CALL /api/search?query=... (or equivalent) and show results
END

ON chat submit (if present)
  POST user message to agent endpoint
  SHOW reply in chat UI
END
```

---

## Summary

Together: **the server exposes JSON APIs backed by SQL**, **SearchService merges entity searches**, **PestControlAgent picks a topic “arm,” pulls DB context, and optionally asks Claude**, and **the static SPA calls those endpoints** to show dashboards, CRUD, search, and chat.
