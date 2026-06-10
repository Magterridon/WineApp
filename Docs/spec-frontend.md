# OBSOLETE
This file is archived and should not be used as source of truth.

# Wine Cellar App — Frontend Specification

## Goal

Build the frontend for the Wine Cellar MVP using Vue 3 and Bootstrap.

The frontend is responsible for:
- login and registration UI
- wines browsing and creation
- cellar management
- recipes browsing and creation
- authenticated routing
- API integration
- basic test coverage
- responsive UI

---

## Stack

- Vue 3
- Vue Router
- Pinia
- Axios
- Bootstrap 5
- Vitest
- Playwright

---

## Main Pages

- Login / Register
- Cellar
- Wines
- Recipes
- Wine Details
- Recipe Details

---

## Navigation

Main navigation should include:
- Cellar
- Wines
- Recipes
- Login / Logout

Authenticated pages should be protected.

---

## Functional Requirements

### Login / Register Page
Features:
- register with email and password
- login with email and password
- handle validation errors
- display success/error messages
- redirect to cellar after successful login

### Cellar Page
Features:
- display all wines in logged-in user cellar
- show bottle count
- search within cellar
- quick add existing wine
- increment bottle count
- decrement bottle count
- remove item when count reaches zero
- link to wine details
- link to recipes/pairings
- show empty state if no wine exists

Each cellar row should show:
- wine name
- domain
- year
- rank
- cepages summary
- drink period
- bottle count

### Wines Page
Features:
- search wines
- list wine cards
- create new wine
- add wine to cellar
- remove wine from cellar
- navigate to wine details

Each wine card should show:
- image
- name
- domain
- year
- short description
- cepages list

### Wine Details Page
Features:
- display all wine fields
- show full cepages list
- show drink period
- show linked recipe pairings

### Recipes Page
Features:
- search recipes
- search by name, ingredient, pairing, type
- list recipe cards
- create new recipe
- navigate to recipe details

Each recipe card should show:
- image
- name
- short description
- recipe type

### Recipe Details Page
Features:
- display full recipe information
- show ingredients
- show instructions
- show wine pairings
- show optional pairing notes

---

## State Management

Use Pinia stores for:
- auth state
- wines state if useful
- cellar state
- recipes state if useful

At minimum:
- auth store
- cellar store

---

## API Integration

Integrate with backend routes:
- `/api/auth/register`
- `/api/auth/login`
- `/api/wines`
- `/api/recipes`
- `/api/cellar`

Use Axios with:
- base API configuration
- JWT token attachment for authenticated requests
- basic error handling

---

## UI Requirements

- use Bootstrap styling
- responsive layout
- simple and clean design
- loading states during API calls
- visible success and error feedback
- reusable components when useful, but do not overengineer

Suggested reusable components:
- navbar
- search bar
- wine card
- recipe card
- loading spinner
- alert message

---

## Forms

### Create Wine Form
Fields:
- name
- domain
- year
- rank
- description
- imageUrl
- drinkFromYear
- drinkToYear
- cepages list with percentage

### Create Recipe Form
Fields:
- name
- description
- ingredients
- instructions
- imageUrl
- recipeType
- paired wines
- pairing notes

---

## Testing Requirements

### Unit tests
Add unit tests for important components/stores:
- auth store
- cellar store
- login form
- create wine form
- create recipe form

### End-to-end tests
Add Playwright tests for:
- register
- login
- browse wines
- create wine
- add wine to cellar
- increment bottle count
- decrement bottle count
- browse recipes
- create recipe

---

## Non-Functional Requirements

- code should be readable and maintainable
- folder structure should be simple
- pages/components separation should be clear
- avoid unnecessary abstraction
- include README frontend setup and test commands if separate

---

## Done Criteria

Frontend is done when:
- all main pages exist
- routing works
- auth works with backend
- cellar flows work
- wine flows work
- recipe flows work
- protected routes work
- tests pass
- UI is responsive and usable