# Wine Cellar App — Backend Specification

## Goal

Build the backend API for the Wine Cellar MVP using ASP.NET Core Web API, Entity Framework Core, PostgreSQL, and JWT authentication.

The backend is responsible for:
- authentication
- wine management
- recipe management
- user cellar management
- validation
- persistence
- test coverage for all API routes

---

## Stack

- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT authentication
- xUnit integration tests

---

## Main Entities

- User
- Cellar
- CellarItem
- Wine
- WineCepage
- Recipe
- RecipeWinePairing

---

## Rules

- A user has exactly one cellar
- A cellar contains many cellar items
- A cellar item references one wine and a bottle count
- Bottle count cannot be negative
- If bottle count reaches 0, delete the cellar item
- Wines are shared across all users
- Wine uniqueness should be enforced using `Name + Domain + Year`
- Recipes and wines have a many-to-many relationship through recipe pairings
- All authenticated users can manage their own cellar
- All authenticated users can create, edit, and delete wines and recipes in MVP

---

## Required API Routes

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`

### Wines
- `GET /api/wines`
- `GET /api/wines/{id}`
- `POST /api/wines`
- `PUT /api/wines/{id}`
- `DELETE /api/wines/{id}`

### Recipes
- `GET /api/recipes`
- `GET /api/recipes/{id}`
- `POST /api/recipes`
- `PUT /api/recipes/{id}`
- `DELETE /api/recipes/{id}`

### Cellar
- `GET /api/cellar`
- `POST /api/cellar/items`
- `PATCH /api/cellar/items/{wineId}/increment`
- `PATCH /api/cellar/items/{wineId}/decrement`
- `DELETE /api/cellar/items/{wineId}`

---

## Search Requirements

### Wines search fields
- name
- domain
- year
- rank
- cepage

### Recipes search fields
- name
- description
- ingredient
- recipe type
- paired wine name

Implement search with query parameters on list endpoints.

---

## Entity Requirements

### User
Fields:
- Id
- Email
- PasswordHash
- CreatedAt

### Cellar
Fields:
- Id
- UserId

### CellarItem
Fields:
- Id
- CellarId
- WineId
- BottleCount

### Wine
Fields:
- Id
- Name
- Domain
- Year
- Rank
- Description
- ImageUrl
- DrinkFromYear
- DrinkToYear
- CreatedAt

### WineCepage
Fields:
- Id
- WineId
- CepageName
- Percentage

### Recipe
Fields:
- Id
- Name
- Description
- Ingredients
- Instructions
- ImageUrl
- RecipeType
- CreatedAt

### RecipeWinePairing
Fields:
- Id
- RecipeId
- WineId
- Notes

---

## Validation Rules

### Auth
- email required
- email must be valid
- password required
- password minimum length required

### Wine
- name required
- domain required
- year required
- imageUrl optional but valid if provided

### WineCepage
- cepageName required
- percentage optional
- if provided, must be between 0 and 100

### Recipe
- name required
- description required
- ingredients required
- instructions required
- imageUrl optional but valid if provided

### Cellar
- bottle count cannot be negative

---

## Auth Requirements

- Use JWT bearer authentication
- Register creates a user and associated cellar
- Login returns token and basic user info
- Protected routes require authentication except login/register

---

## DTO Requirements

Use DTOs for:
- auth requests/responses
- wine create/update/read
- recipe create/update/read
- cellar read/update

Do not expose EF entities directly from controllers.

---

## Testing Requirements

Write integration tests for every route:
- success cases
- validation failures
- unauthorized cases
- not found cases

Include test coverage for:
- register
- login
- create wine
- update wine
- delete wine
- create recipe
- update recipe
- delete recipe
- get cellar
- increment bottle
- decrement bottle
- remove bottle completely

---

## Database Requirements

- Use EF Core migrations
- Configure PostgreSQL
- Seed a few wines and recipes for development/testing

---

## Non-Functional Requirements

- clean architecture, but do not overengineer
- simple service layer is enough
- readable controller/service/repository separation if useful
- consistent API responses and error handling
- include README setup instructions

---

## Done Criteria

Backend is done when:
- app runs locally
- database migrations work
- authentication works
- all required routes exist
- validation is implemented
- tests pass
- seed data exists
- README explains how to run the backend and tests