# Wine Cellar App — Shared Search and Filter Specification

## Goal

Define a single consistent search and filter pattern to be used across the app.

The search/filter experience should be:
- consistent across pages
- more explicit than the current single generic text search
- easy to understand
- modern and practical
- accessible

This specification should be used anywhere the app displays searchable/filterable wine lists or wine-related tables.

---

## Core principle

The app should no longer rely primarily on one broad free-text search that searches everything at once.

Instead, the app should use explicit search/filter categories with a shared UI pattern across pages.

The same search/filter structure should be reused everywhere it makes sense.

---

## Global consistency rule

All pages that expose wine search/filtering should use the same search/filter model and visual structure as much as possible.

This includes, where relevant:
- cellar pages
- wine catalog/list pages
- admin wine backoffice pages
- any wine table/list view using shared filtering behavior

If a page cannot support every filter for technical or product reasons, it should still follow the same overall pattern and naming.

---

## Search and filter fields

## 1. Name
### Type
Text input

### Behavior
- dedicated text input
- searches wine name only
- partial matching supported if practical

### Label
Name

---

## 2. Domain
### Type
Text input

### Behavior
- dedicated text input
- searches producer / domain / winery field only
- partial matching supported if practical

### Label
Domain

### Notes
If the current schema uses producer or winery instead of domain, map this filter to the correct existing field without changing the user-facing intention.

---

## 3. Appellation
### Type
Text input

### Behavior
- dedicated text input
- searches appellation only
- partial matching supported if practical

### Label
Appellation

---

## 4. Cépage
### Type
Multi-select dropdown with search box

### Behavior
- dropdown contains a search text box
- user can search available cépages within the dropdown
- multiple cépages can be selected
- selected values should remain clearly visible
- filtering logic should be clearly defined and consistent:
  - use OR matching by default unless current app logic strongly requires AND
- keep implementation simple and maintainable

### Label
Cépage

### Notes
If the current schema uses grape varieties or a similar field, map this control to that field.

---

## 5. Color
### Type
Multi-select visual filter

### Behavior
- display wine color options as clickable items
- each option should show:
  - a bottle icon
  - a visible color
  - a text label
- multiple colors can be selected
- selected options should have a clear active state

### Accessibility
- do not rely on color alone
- always show text labels for color-blind accessibility

### Label
Color

### Example values
- Red
- White
- Rosé
- Sparkling
- Orange
- Other

Only use values supported by the current model/data.

---

## 6. Pairing
### Type
Searchable dropdown

### Behavior
- keep pairing search as a dropdown with search text input
- dropdown should search across all available pairings/recipes available in the site
- user selects one pairing value at a time unless multi-select is already supported and clearly useful
- preserve current working behavior unless a strong reason exists to improve it now

### Label
Pairing

---

## Sorting behavior

### Rule
Sorting should be done directly from the table headers.

### Requirements
- users sort by clicking a column header
- clicking once sorts ascending
- clicking again sorts descending
- active sort column and direction should be visually clear
- sorting controls should not be duplicated elsewhere unless already required for mobile UX

### Notes
Do not build a separate sort dropdown if header sorting is sufficient.

---

## Layout and UX expectations

### General layout
- filters should be grouped clearly
- keep the UI readable
- keep the same order of fields across pages where practical
- avoid clutter

### Preferred field order
1. Name
2. Domain
3. Appellation
4. Cépage
5. Color
6. Pairing

If a page needs fewer filters, preserve this order for the filters that are shown.

### Interaction expectations
- filters should be easy to reset/clear
- selected values should be clearly visible
- filter behavior should feel predictable across the app
- changing filters should update the list/table consistently

---

## Shared component expectation

The search/filter UI should be implemented as shared/reusable components or a shared pattern wherever practical.

Goal:
- reduce duplication
- improve consistency
- make future changes easier

Do not overengineer, but prefer reuse over page-by-page custom implementations.

---

## Backend / query expectations

- backend filtering should align with the explicit UI filters
- do not depend on one catch-all query when separate filters exist
- preserve partial matching for text fields where useful
- support multi-select handling for:
  - cépage
  - color
- preserve current permissions and page-specific data visibility rules

---

## Accessibility expectations

- labels must be clear
- color options must include text labels
- controls should remain usable by keyboard where practical
- active/selected states should be visually clear
- dropdowns should remain understandable and not overly complex

---

## Non-goals for this change

This specification does not require:
- advanced saved searches
- natural language search
- AI search
- fuzzy semantic search
- a full faceted search engine
- deep redesign of the domain model unless strictly needed

---

## Definition of Done

This work is successful when:
- the app no longer depends on a single generic wine search bar as the main search UX
- wine search/filtering uses explicit categories
- the same search/filter structure is used consistently across relevant pages
- Name, Domain, and Appellation are separate text fields
- Cépage is a searchable multi-select dropdown
- Color is a multi-select visual filter with bottle icon + color + text
- Pairing remains a searchable dropdown across site pairings/recipes
- sorting is performed via clickable table headers
- the implementation is reusable, maintainable, and consistent across pages