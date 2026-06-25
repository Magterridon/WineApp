# Wine Cellar App — Cellar Specification

## Goal

Provide a personal cellar page where the user can manage owned bottles, understand what should be drunk, and review tasting history.

---

## Main page

### Page name
`My Cellar`

### Core responsibilities
The cellar page should let the user:
- view all wines currently in their cellar
- understand bottle counts
- search and filter cellar wines
- add bottles of existing wines
- add an existing catalog wine into the cellar
- increment/decrement bottle counts
- review tasting history
- understand drinking readiness

---

## Top summary area

### Goal
Provide immediate insight into the cellar.

### Summary cards/panels
The cellar page should show top summary panels such as:
- Total Bottles
- Ready to drink
- Drink Soon
- Past Peak

### Behavior
These panels should act as interactive filters.

When a user clicks a panel:
- that panel becomes the active filter
- the cellar wine list updates
- tasting history updates consistently where practical
- active state is clearly visible

### Meaning
- Total Bottles: all cellar wines and all tasting history
- Ready to drink: only ready wines and matching tasting history where feasible
- Drink Soon: only drink-soon wines and matching tasting history where feasible
- Past Peak: only past-peak wines and matching tasting history where feasible

`Too Young` can remain available in filter logic even if not displayed as a top card.

---

## Cellar wine list

### Goal
Show wines the user currently owns.

### Each row/card should show useful information such as:
- image if available
- wine name
- domain / producer
- year
- appellation / region if available
- color if available
- rank/classification if available
- cépage summary if available
- drink window
- drink status
- bottle count

### Main actions
- increment bottle count
- decrement bottle count
- remove from cellar if needed
- open wine details

---

## Search and filtering

### Goal
Make the cellar practical for everyday use.

### Filters should support:
- Name
- Domain
- Appellation
- Cépage
- Color
- Pairing
- Year / vintage where useful
- drink status
- drink window inclusion such as “drink this year” if supported

### Additional cellar-specific filter behavior
- top summary panels act as primary status filters
- search/filter combinations should work together consistently

---

## Add wine / add bottle flows

### Supported actions
The user should be able to:
- add bottles of a wine already in their cellar
- add an existing wine from the shared catalog into their cellar
- optionally create a new wine if current product rules still allow it

### UX expectations
These actions should be easy to discover and not feel buried.

---

## Drinking guidance

### Goal
Help the user decide what to open.

### Statuses
Where supported, display clear drink statuses such as:
- Too Young
- Ready to drink
- Drink Soon
- Past Peak

### Logic
Use current drink-window logic as source of truth.
Do not duplicate contradictory logic across summary cards, list rows, and details.

---

## Tasting history

### Goal
Show what the user has tasted and recorded.

### Scope
The cellar page includes a tasting history section with the user's tasting notes.

### Each tasting history item may show, where supported:
- wine reference
- tasting date
- rating
- tasting note
- meal used
- pairing rating
- pairing note

### Filtering behavior
Tasting history should update consistently with cellar filtering where practical, especially for top summary panel filters.

---

## Empty and partial states

The page should handle:
- empty cellar
- wines without images
- incomplete metadata
- no tasting history yet

These states should remain clear and friendly.

---

## Definition of Done

This feature is successful when:
- users can see all wines in their cellar
- users can manage bottle counts
- users can search/filter cellar wines clearly
- top summary cards act as filters
- users can understand drink readiness
- tasting history is visible and useful
- cellar and tasting history feel connected and coherent