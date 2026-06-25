# Wine Cellar App — Meals Specification

## Goal

Provide a shared meals page where users can browse meals, search them, create new meals, and understand which wines pair with them.

---

## Terminology

Use one consistent term in the product UI where practical.

Preferred term:
- `Meals`

If current implementation uses `Recipes`, the data model may keep that naming internally, but product UI should become consistent over time.

---

## Main pages and flows

- meals list page
- meal details page
- create meal flow
- meal pairing visibility

---

## Meals list page

### Goal
Show all meals available in the app.

### Core behavior
The page should provide:
- a list/grid/table of meals
- search/filter controls
- a clear action to create a new meal
- navigation to meal details

### Main actions
- browse meals
- search/filter meals
- sort meals if practical
- open meal details
- create meal

---

## Search and filtering

### Useful filters
- meal name
- ingredient
- meal/category/type
- paired wine
- paired appellation/style if supported

### Behavior
- keep search practical and understandable
- avoid turning meals search into an overly complex filter builder

---

## Meal list content

Each meal card/row should show useful information such as:
- image
- name
- short description
- meal type/category if supported
- pairing hint/count if useful

---

## Create meal action

### Goal
Allow creation of a new meal.

### Requirements
- clear create action on meals page
- form should support current meal/recipe model
- validation should be clear

---

## Meal details page

### Goal
Show full useful information for one meal.

### The page should show
- image
- name
- description
- ingredients
- instructions if supported by the current model
- linked wines/pairings
- pairing notes if supported

### Pairing visibility
Meal detail should clearly show wines that pair with the meal.

---

## Pairing relationship

Meals can pair with multiple wines.
Wines can pair with multiple meals.

The meals feature should work cleanly with:
- direct pairings
- future/admin-managed pairing rules where relevant to presentation

---

## Definition of Done

This feature is successful when:
- users can browse all meals
- users can search meals
- users can create a meal
- users can view meal details
- users can understand which wines pair with a meal