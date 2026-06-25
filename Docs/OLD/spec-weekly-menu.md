# Wine Cellar App — Weekly Menu Feature Specification

## Goal

Create a new page called `Weekly Menu`.

This page should present a curated 3-course menu built from the user's cellar, pairing each course with a wine that is ready to drink.

The page should feel premium, simple, and inspiring.

This first version should focus primarily on frontend experience and rendering, while keeping backend/data logic simple and maintainable.

---

## Core concept

The `Weekly Menu` page proposes a complete dinner menu for the user:
- entry / starter
- main course
- dessert

Each course is paired with one wine from the user's cellar.

For each row:
- wine appears on the left
- meal appears on the right

The goal is to answer:
**"What nice meal could I make this week with bottles that are ready to drink in my cellar?"**

---

## Scope for this iteration

### In scope
- new `Weekly Menu` page
- 3-course layout:
  - starter
  - main course
  - dessert
- one paired wine per course
- rich card-based presentation
- clear side-by-side wine/meal layout
- use existing available wine/recipe/pairing data where possible
- keep implementation mainly frontend-focused if backend logic is still evolving

### Optional / phase 2
- automatic weekly refresh every Sunday at 00:00
- manual regenerate action
- multiple weekly menu variants
- AI-generated explanations
- advanced seasonal logic

### Recommendation
For MVP, the weekly scheduled refresh should be treated as optional.
If it adds too much complexity, do not include it in this iteration.

---

## Page name and route

### Page name
`Weekly Menu`

### Route
Use the route naming/style already used by the app.
If no strong convention exists, prefer something simple like:
- `/weekly-menu`

---

## Layout requirements

## Overall page structure
The page should display three menu sections:
1. starter / entry
2. main course
3. dessert

Each section should be presented as a full-width card or row.

Each row must contain:
- wine card/content on the left
- meal card/content on the right

The row should feel balanced and visually premium.

---

## Per-row structure

## Left side: Wine
For each selected wine, show as much useful existing information as practical, including:

- wine image
- wine name
- producer / domain / château
- vintage
- appellation / region
- color / type
- cépages if available
- drinking window / status if available
- short wine description
- useful wine characteristics already supported by the current model

### Notes
- preserve current schema as source of truth
- do not invent unsupported wine fields
- if some wine fields are missing in data, degrade gracefully

---

## Right side: Meal
For each meal, show:

- course label:
  - starter / entry
  - main course
  - dessert
- recipe name
- recipe description
- ingredient list

### Notes
- ingredients can be displayed as text list or bullet list
- keep it readable and elegant
- use existing recipe data if available
- if recipe fields are incomplete, degrade gracefully

---

## Visual expectations

### Design goals
The page should feel:
- elegant
- modern
- appetizing
- easy to scan

### Card behavior
- each course should be displayed in a strong horizontal card/section
- wine and meal should align on the same row on desktop
- on smaller screens, stacking is acceptable if responsive behavior requires it

### Content emphasis
- wine image should be clearly visible
- recipe title and description should be prominent
- the pairing should feel intentional and premium

---

## Data expectations

## Menu composition
The page should display exactly:
- 1 starter + 1 paired wine
- 1 main course + 1 paired wine
- 1 dessert + 1 paired wine

## Source of wines
Wines should come from the user's cellar / cave.

## Pairing principle
Use existing pairing logic/data where possible.
This iteration does not require a perfect sommelier engine.

If pairing logic is still limited, use the best practical current logic available.

## Readiness principle
Prefer wines that are ready to drink if that information exists.

If drink-window/readiness logic is incomplete, use the best available approximation already present in the system.

---

## Refresh behavior

## Desired product behavior
The weekly menu should refresh every Sunday at 00:00.

## MVP rule
This automatic refresh is optional in this iteration.

If implementing scheduled refresh would add too much complexity:
- do not include it yet
- structure the feature so it can be added later

Possible acceptable MVP behavior:
- page displays current generated weekly menu
- generation is static, seeded, manual, or based on current data
- implementation is compatible with future weekly regeneration

---

## Frontend-first implementation guidance

This feature should primarily focus on:
- page structure
- layout
- component design
- rendering of wine + meal pairs

Backend logic should remain as simple as possible for this first iteration.

If needed, use:
- mock/fallback composition logic
- existing APIs
- existing recipe/pairing/wine relations

Avoid overengineering the generation engine in this phase.

---

## Empty and partial states

### If not enough data exists
Handle gracefully if:
- the user has too few wines
- the user has no wines with images
- the user has incomplete pairings
- a course cannot be generated properly

Possible fallback behavior:
- show placeholders
- show incomplete but readable cards
- show a clear empty-state message
- avoid page-breaking failures

### Empty-state guidance
If there is not enough data to build a full weekly menu, explain what is missing in a friendly way.

Example missing data:
- no wines in cellar
- no ready-to-drink wines
- no recipes/pairings available

---

## Technical constraints

- review current implementation first
- preserve existing schema as much as possible
- use existing components/styles/patterns where practical
- keep frontend maintainable
- avoid large backend refactors in this iteration
- do not introduce unnecessary complexity
- update tests if needed

---

## Definition of Done

This work is successful when:
- a `Weekly Menu` page exists
- the page shows 3 courses:
  - starter
  - main course
  - dessert
- each course is paired with one wine from the user's cellar
- each row shows wine on the left and meal on the right
- wine information includes image and useful characteristics where available
- meal information includes recipe, description, and ingredients
- the layout feels polished and usable
- the page handles incomplete data gracefully
- weekly Sunday auto-refresh is either implemented simply or intentionally deferred