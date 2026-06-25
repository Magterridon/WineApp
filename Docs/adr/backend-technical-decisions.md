# ADR — Backend Technical Decisions

## Status
Accepted

## Context

The project is a Wine Cellar MVP application with:
- an ASP.NET Core Web API backend
- PostgreSQL persistence
- JWT authentication
- a Vue frontend client

The backend already matches the product needs reasonably well.
The current product pressure is mostly on:
- frontend quality
- pairing logic
- admin tooling
- maintainable feature evolution

The backend should remain stable and simple while still supporting:
- wine catalog management
- cellar management
- meals/recipes
- direct pairings
- future pairing rules
- future curated features such as Weekly Menu

---

## Decision

The backend will:
- continue using **ASP.NET Core Web API**
- continue using **Entity Framework Core**
- continue using **PostgreSQL**
- continue using **JWT bearer authentication**
- remain an **API-first backend** consumed by the frontend
- preserve a **simple maintainable architecture** rather than introducing unnecessary complexity

The backend should evolve incrementally to support:
- richer wine metadata
- meals/recipes
- direct pairings
- admin-only curation endpoints
- pairing rule management
- future weekly-menu support

---

## Rationale

### Why keep ASP.NET Core
- it already fits the project
- it is a strong choice for API development
- it aligns with long-term interest in the Microsoft/Azure ecosystem
- there is no current product reason to replace it

### Why keep PostgreSQL
- it is a strong fit for structured relational data
- the app relies on many relational concepts:
  - wines
  - cellar items
  - meals
  - pairings
  - pairing rules
- it supports future growth without forcing premature complexity

### Why keep EF Core
- it supports fast MVP iteration
- it works well with migrations
- it is sufficient for the project’s current complexity

### Why keep JWT auth
- it is already suitable for SPA + API interaction
- it keeps frontend/backend responsibilities clear
- it is enough for MVP and admin-role use cases

### Why keep API-first separation
- the frontend and backend can evolve separately
- it avoids coupling UI redesign to backend rendering concerns
- it keeps the system compatible with future infrastructure decisions

---

## Consequences

### Positive
- stable backend foundation
- lower migration risk
- easier incremental feature development
- compatible with future Azure deployment if desired
- fits current app and team learning goals

### Tradeoffs
- some richer pairing/recommendation features may require additional schema evolution
- image/file handling should eventually move away from local disk assumptions
- admin and pairing-rule features may increase backend complexity over time if not kept disciplined

---

## Implementation guidance

- preserve current working API behavior where possible
- evolve schema through migrations
- use DTOs rather than exposing persistence entities directly
- keep role protection explicit for admin endpoints
- add backend support only where it creates real product value
- avoid overengineering service layers or abstractions
- support frontend needs with clear filtering/query patterns

---

## Known future concerns

### Image storage
Local file storage is acceptable only as a temporary/dev-stage solution.
Long term, images should move to object storage such as:
- Azure Blob Storage
- S3-compatible storage
- Cloudinary

### Pairing expansion
The backend should support:
- direct wine-meal pairings
- deterministic pairing rule evaluation
without becoming a heavy recommendation engine too early.

### Weekly Menu
Weekly Menu support should remain simple at first and grow from existing pairing/cellar data rather than requiring complex generation infrastructure immediately.

---

## Out of scope for this decision

This ADR does not decide:
- exact cloud hosting provider
- object storage provider
- event-driven architecture
- microservices
- recommendation engine architecture