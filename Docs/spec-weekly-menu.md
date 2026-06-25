# Wine Cellar App — Weekly Menu Specification

## Goal

Provide a curated `Weekly Menu` page that proposes a three-course meal paired with wines from the user's cellar.

The page should feel premium, inspiring, and easy to understand.

---

## Core concept

The `Weekly Menu` proposes:
- starter
- main course
- dessert

Each course is paired with one wine from the user's cellar.

For each row:
- wine is shown on the left
- meal is shown on the right

The page should answer:
**"What nice meal could I make this week with bottles that are ready to drink in my cellar?"**

---

## Scope

### In scope
- Weekly Menu page
- three-course layout
- one paired wine per course
- rich side-by-side presentation
- use cellar wines
- use existing direct pairings and/or pairing logic where practical
- frontend quality is a high priority

### Optional / later
- weekly regeneration every Sunday at 00:00
- manual regenerate action
- multiple menu variants
- richer selection logic
- AI-assisted explanations

---

## Route

Use current route conventions.
If none is established, prefer:
- `/weekly-menu`

---

## Layout

### Overall structure
The page shows exactly three course sections:
1. starter
2. main course
3. dessert

Each section is a full-width horizontal card/row.

### Row composition
- left side: wine
- right side: meal

Desktop should use side-by-side layout.
Smaller screens may stack responsively if needed.

---

## Wine presentation

Show as much useful existing wine information as practical, such as:
- image
- name
- domain / château / producer
- vintage
- appellation / region
- color / type
- cépages if available
- drink window / status if available
- description
- useful existing characteristics

Degrade gracefully if some metadata is missing.

---

## Meal presentation

Show:
- course label
- meal name
- description
- ingredients

If current meal data supports more, it can also include:
- image
- meal type/category
- instructions preview if appropriate

Degrade gracefully if some data is incomplete.

---

## Data and pairing logic

### Source of wines
Wines should come from the user's cellar.

### Readiness
Prefer wines that are ready to drink where that information exists.

### Pairing source
Use the best available current logic, such as:
- direct pairings
- pairing rules if implemented
- fallback practical matching if needed

This page does not require a perfect sommelier engine.

---

## Refresh behavior

### Desired long-term behavior
The menu refreshes every Sunday at 00:00.

### MVP behavior
Automatic weekly refresh is optional.
If too complex, it may be deferred.

Acceptable MVP options include:
- current generated menu
- seeded/static selection
- manual/internal generation
- implementation prepared for later weekly regeneration

---

## Empty and partial states

Handle gracefully when:
- the cellar is empty
- no wines are ready to drink
- not enough pairings exist
- not enough meal data exists

The page should remain understandable and inviting even with incomplete data.

---

## Visual expectations

This page should receive high UI polish.

It should feel:
- elegant
- modern
- appetizing
- premium
- intentional

The pairing should feel curated, not random.

---

## Definition of Done

This feature is successful when:
- a Weekly Menu page exists
- it shows starter, main course, and dessert
- each course is paired with one cellar wine
- wine is shown on the left and meal on the right
- the page is polished and usable
- incomplete data is handled gracefully
- weekly refresh is either implemented simply or intentionally deferred