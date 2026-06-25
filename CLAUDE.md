# Claude Project Context

## Project
This repository contains a Wine Cellar MVP web application.

The goal is to build a simple full-stack app where users can:
- register and log in
- browse wines
- create wines
- manage their personal cellar
- browse recipes
- create recipes
- view wine pairings for recipes
- discover curated wine-and-meal experiences such as Weekly Menu

---

## Product Priorities
- MVP only
- working software over perfect architecture
- simple and maintainable implementation
- avoid overengineering
- clean code
- clear folder structure
- readable tests
- fast local setup
- elegant user experience on important user-facing pages
- consistency across frontend patterns and interactions

---

## Technical Decisions
### Frontend
- Vue 3
- Vue Router
- Pinia
- Axios
- custom application styling
- shared reusable UI components
- Bootstrap must not be used

### Backend
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT authentication

### Testing
- Frontend unit tests: Vitest
- Frontend end-to-end tests: Playwright
- Backend tests: xUnit integration tests

---

## Product and UX Direction

The product should not feel like a generic admin dashboard or a bootstrap CRUD application.

The intended product feel is:
- modern
- elegant
- premium
- warm
- refined
- consumer-facing
- wine/lifestyle-oriented

Frontend decisions should prioritize:
- clean typography hierarchy
- strong spacing rhythm
- restrained use of color
- polished cards, lists, filters, and forms
- consistent reusable UI patterns
- accessibility and readability
- responsive layouts
- clear visual hierarchy

Avoid:
- Bootstrap
- bootstrap-style components or layout utilities
- generic dashboard/admin template styling
- overly technical backoffice feeling on user-facing pages
- excessive visual clutter
- heavy borders everywhere
- inconsistent spacing and typography
- page-by-page one-off styling without shared patterns

Admin pages can remain simpler than customer-facing pages, but should still stay visually coherent with the overall product.

---

## Domain Model
Main entities:
- User
- Cellar
- CellarItem
- Wine
- WineCepage
- Recipe
- RecipeWinePairing

Key rules:
- one user has exactly one cellar
- cellar items track bottle count per wine
- if bottle count reaches 0, delete the cellar item
- wines are shared across users
- wine uniqueness is based on Name + Domain + Year
- recipes and wines are many-to-many through recipe pairings

The product may also evolve to include:
- pairing rules managed by admins
- curated weekly menu experiences
- richer wine/meal suggestion logic

---

## Auth and Authorization
- users authenticate with email and password
- JWT bearer auth
- register creates both the user and an empty cellar
- authenticated users can:
  - view all wines
  - view all recipes
  - manage only their own cellar
- for MVP, authenticated users can also create, edit, and delete wines and recipes

---

## MVP Scope
Include:
- register/login
- wines CRUD
- recipes CRUD
- cellar management
- search on wines and recipes
- protected frontend routes
- seed data
- tests
- README

Can also include if already in progress or explicitly requested:
- Weekly Menu
- pairing rules
- curated pairing experiences

Do not include unless explicitly requested:
- social features
- generic dashboard-style admin redesigns
- image upload storage infrastructure redesign
- barcode scanning
- heavy AI recommendation engine
- external API integrations

---

## Implementation Guidance
- prefer simple CRUD patterns where CRUD is the right fit
- use DTOs between API and client
- do not expose persistence entities directly
- keep frontend components simple, reusable, and visually coherent
- prefer server-side filtering through query parameters where reasonable
- add validation on both backend and frontend where reasonable
- use migrations for database schema
- seed sample wines and recipes
- prefer shared UI components/patterns over one-off page-specific solutions
- preserve current working behavior unless a change is explicitly required
- for user-facing pages, prioritize elegance and clarity, not just functionality
- do not add Bootstrap or keep Bootstrap dependencies in the frontend

---

## Frontend UI Guidance
- Bootstrap must not be used anywhere in the frontend
- remove existing Bootstrap dependencies, classes, and patterns where relevant to the task
- do not replace Bootstrap with another generic admin template look
- prefer custom styling and shared design patterns
- use a restrained palette and consistent spacing scale
- prioritize polish on:
  - cellar pages
  - wine list/detail pages
  - recipe pages
  - pairing experiences
  - Weekly Menu
- tables, filters, forms, and cards should feel coherent across the app
- when improving UI, do it systematically, not as isolated patches

---

## Working Style
- before making large changes, understand existing structure
- keep naming consistent
- make small coherent changes
- keep tests updated
- do not introduce unnecessary libraries
- do not redesign the stack without explicit instruction
- when making UI changes, review existing shared styles/components first
- prefer extending and improving existing work rather than replacing it blindly

---

## Suggested Folder Intent
This repo may contain:
- `docs/spec-backend.md`
- `docs/spec-frontend.md`
- `docs/spec-search-and-filters.md`
- `docs/spec-weekly-menu.md`
- `docs/spec-pairing-rules.md`
- backend project files
- frontend project files

If both frontend and backend are in the same repo, keep them clearly separated.

---

## Definition of Done
A task is complete only if:
- implementation works
- relevant tests pass
- no broken auth flow
- no broken cellar flow
- no broken wine/recipe creation flow
- code remains readable
- UI changes improve consistency, not reduce it
- no new Bootstrap usage is introduced

---

## Agent skills

### Issue tracker

Issues live in GitHub Issues for this repo. See `docs/agents/issue-tracker.md`.

### Triage labels

Default label vocabulary (needs-triage, needs-info, ready-for-agent, ready-for-human, wontfix). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context layout — one `CONTEXT.md` + `docs/adr/` at the repo root. See `docs/agents/domain.md`.