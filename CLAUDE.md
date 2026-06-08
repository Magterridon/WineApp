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

---

## Technical Decisions
### Frontend
- Vue 3
- Vue Router
- Pinia
- Axios
- Bootstrap 5

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

Do not include:
- social features
- admin panel
- image upload storage
- barcode scanning
- recommendation engine
- external API integrations

---

## Implementation Guidance
- prefer simple CRUD patterns
- use DTOs between API and client
- do not expose persistence entities directly
- keep frontend components simple
- prefer server-side filtering through query parameters
- add validation on both backend and frontend where reasonable
- use Bootstrap for layout and styling
- use migrations for database schema
- seed sample wines and recipes

---

## Working Style
- before making large changes, understand existing structure
- keep naming consistent
- make small coherent changes
- keep tests updated
- do not introduce unnecessary libraries
- do not redesign the stack without explicit instruction

---

## Suggested Folder Intent
This repo may contain:
- `docs/spec-backend.md`
- `docs/spec-frontend.md`
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