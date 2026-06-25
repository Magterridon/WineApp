# Wine Cellar App — Wine Catalog Specification

## Goal

Provide a shared wine catalog where users can browse all wines, search them easily, inspect details, and add relevant wines to their cellar.

---

## Main pages and flows

- wine list page
- wine details page
- create wine flow
- add to cellar actions from relevant wine views

---

## Wine list page

### Goal
Show all wines available in the shared catalog.

### Core behavior
The page should provide:
- a wine list/table/grid of existing wines
- search/filter controls
- sorting
- a clear action to create a new wine
- a clear action to add a wine to the user's cellar where relevant

### Main actions
- browse wines
- search/filter wines
- sort wines
- open a wine detail
- create a new wine
- add a bottle / add to cellar

---

## Search and filtering

### Shared rule
Wine search should use explicit search/filter fields instead of one broad generic text search.

### Minimum filters
- Name
- Domain
- Appellation
- Cépage
- Color
- Pairing

### Optional filters where supported
- Year / vintage
- Region
- Country
- Rank / classification
- Drink window / drink status
- image presence if useful for admin-like contexts only

### Filter behavior
- Name: text input, wine name only
- Domain: text input, producer/domain only
- Appellation: text input
- Cépage: searchable multi-select dropdown
- Color: multi-select visual filter with bottle icon + color + text
- Pairing: searchable dropdown over available meals/pairings

### UX expectations
- filters should be clear and consistent
- selected values should be visible
- reset/clear should be easy
- filters should not overload the page

---

## Sorting

### Rule
Sorting should be done from the table/list header or equivalent clear controls.

### Useful sortable fields
- name
- domain
- year
- appellation
- rank if supported
- drink window status if supported

### Behavior
- click once: ascending
- click again: descending
- active sort should be visible

---

## Wine list content

Each wine row/card should show as much relevant information as practical, such as:
- image
- name
- domain / château / producer
- year
- appellation / region
- color
- cépage summary
- description snippet
- drink status if available

The exact display can vary by page layout, but the information should remain coherent.

---

## Create wine action

### Goal
Allow creation of a new wine when it does not already exist.

### Requirements
- clear button/action on wine list page
- form should support current wine model
- validation should be clear

### Duplicate prevention
Before creating a new wine, the app should encourage reuse of existing wines where practical.

At minimum:
- search existing catalog first
- avoid obvious duplicate creation

---

## Wine details page

### Goal
Show complete useful information for one wine.

### The page should show
- image
- full wine identity fields
- description
- cépages
- drink window
- drink status if supported
- linked meals/pairings
- relevant cellar action if useful

### Pairing visibility
Wine detail should clearly show meals this wine pairs with.

If direct and rule-based pairings both exist later, the page should still present results clearly.

---

## Add to cellar actions

### Goal
Make it easy to add a catalog wine to the user's cellar.

### Behavior
Depending on current UX patterns, the user should be able to:
- add a wine to cellar
- add bottle count
- navigate to cellar if needed after action

---

## Data expectations

Wine fields should support the current model, including where available:
- name
- domain
- year
- rank
- description
- imageUrl
- drinkFromYear
- drinkToYear
- appellation
- region
- country
- color
- cépages
- classification if supported

Do not invent unsupported fields unless explicitly requested.

---

## Definition of Done

This feature is successful when:
- users can browse all wines
- users can search/filter wines clearly
- users can sort wines
- users can create a wine
- users can open wine details
- users can add relevant wines to their cellar
- wine pages feel coherent and useful