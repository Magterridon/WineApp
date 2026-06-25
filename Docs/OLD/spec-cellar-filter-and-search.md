# Wine Cellar App — Cellar Filter and Search Improvement Spec

## Goal

Improve the cellar page so that:
- the top summary panels become interactive filters
- the displayed wine list updates from the selected panel
- the tasting history also updates consistently
- users can search and filter wines more precisely

This should improve usability and make the cellar page more practical for day-to-day use.

---

## Current UX issue

The page currently has:
- top summary panels such as:
  - Total Bottles
  - Ready to drink
  - Drink Soon
  - Past Peak
- a separate filter section with status filters such as:
  - All
  - Ready
  - Drink Soon
  - Past Peak
  - Too Young

This feels redundant.

---

## Desired UX change

### Remove or replace the redundant status filter section
The top summary panels should become the main status-based filter control.

### Interactive top panels
The following top panels should be clickable:
- Total Bottles
- Ready to drink
- Drink Soon
- Past Peak

### Filtering behavior
When the user clicks one of these panels:
- that panel becomes the active filter
- the current cellar wine list updates accordingly
- the tasting history section also updates consistently according to the selected filter
- the active panel should be visually highlighted

### Expected meaning of each panel
- Total Bottles:
  - show all current cellar wines
  - show all tasting history entries
- Ready to drink:
  - show only wines currently in cellar with status "Ready to drink"
  - show only tasting history entries related to wines with that same drink-window status
- Drink Soon:
  - show only wines currently in cellar with status "Drink Soon"
  - show only tasting history entries related to wines with that same drink-window status
- Past Peak:
  - show only wines currently in cellar with status "Past Peak"
  - show only tasting history entries related to wines with that same drink-window status

### Too Young
- keep support for "Too Young" in search/filtering if useful
- but it does not need to appear as a top panel at this stage unless the UI already supports it naturally

---

## Search and Filter Improvements

## Goal
Allow users to find wines more easily using practical search criteria.

### The cellar page search/filter should support:
- wine name
- producer/domain
- vintage/year
- rank
- status
- drink window
- meal
- ready/past-peak style filtering via the top panels

### Search/filter details

#### Wine name
- text search
- partial match supported if possible

#### Producer / domain
- text search
- partial match supported if possible

#### Year / vintage
- exact year filter

#### Rank
- filter by wine rank if that field exists in the current model
- if rank does not exist or is not yet usable, review whether to add it or postpone it

#### Status
- support filtering by drink status such as:
  - Too Young
  - Ready to drink
  - Drink Soon
  - Past Peak

#### Drink window
- support a practical option such as:
  - checkbox: "This year"
- meaning:
  - show wines whose drink window includes the current year
  - if a better existing drink-window logic is already present, integrate with it

#### Meal
- support filtering by linked recipe/meal
- if relevant, include both:
  - official linked recipe/meal
  - custom meal data from drink history if this is already modeled and feasible

---

## Data and logic expectations

### Drink status logic
Use the existing drink-window logic as the source of truth.
Do not duplicate inconsistent logic between cards and lists.

### Tasting history filtering
When filtered by top panel or search options, tasting history should remain consistent with the same criteria where feasible.

### Minimal-change rule
Review the current implementation first and make only the necessary schema/API/UI changes.

---

## UI expectations

### Top panel behavior
- clickable
- active state clearly visible
- works like a filter control
- should feel faster and simpler than the old separate filter tabs/buttons

### Search/filter UX
- keep it simple and readable
- avoid too many confusing controls
- group related filters clearly
- preserve mobile usability if possible

### Page behavior
- changing a top panel filter updates both:
  - current cellar wine list
  - tasting history section
- search/filter combinations should work together consistently

---

## Technical expectations

- review current cellar page implementation
- review current API/query/filter support
- add or refine backend filtering if needed
- update frontend state management if needed
- preserve existing working features
- update tests where needed

---

## Definition of Done

This work is successful when:
- the top summary panels are clickable and act as the main status filters
- the separate redundant status section is removed or no longer needed
- the wine list updates correctly from the selected top panel
- the tasting history updates consistently too
- users can search/filter by:
  - wine name
  - producer/domain
  - year
  - status
  - drink window
  - meal
  - rank if supported
- the page feels clearer and more useful than before