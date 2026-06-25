# Wine Cellar App — Admin Wine Backoffice Specification

## Goal

Provide an admin-only backoffice for curating and improving the shared wine catalog.

This tool is intended for internal catalog curation, not as a public-facing feature.

---

## Main objectives

Admin users should be able to:
- find wines quickly
- filter and sort wines efficiently
- select one or many wines
- improve metadata
- manage images
- manage direct pairings
- perform practical bulk curation tasks

---

## Access control

- only Admin users can access this feature
- admin permissions must be enforced on the backend
- non-admin users must not see or access these pages/actions

---

## Main page

### Goal
Provide a practical searchable admin wine list for curation.

### Core behavior
The page should support:
- wine listing
- search and filtering
- sorting
- row selection
- bulk actions
- single wine quick-edit access

---

## Search and filtering

### Goal
Help Admin quickly find groups of wines.

### Useful filters
- Name
- Domain
- Appellation
- Cépage
- Color
- Pairing
- Year / vintage
- Country
- Region
- rank / classification
- drink status
- image presence
- pairing presence
- source/import source if available

### Notes
- use existing schema as source of truth
- keep filtering practical
- preserve consistency with shared search/filter patterns where relevant

---

## Sorting

Useful sorting should include where practical:
- name
- domain
- year
- appellation
- rank/classification
- region
- updated date if available
- image presence if practical

---

## Selection and bulk actions

### Goal
Allow efficient catalog curation.

### Requirements
Admin should be able to:
- select one wine
- select multiple wines
- select all visible results on the current page
- keep selection predictable while filtering where practical

### Bulk actions
Support practical bulk actions such as:
- bulk metadata updates
- bulk image assignment
- bulk direct pairing assignment

---

## Image management

### Goal
Improve wine images efficiently.

### Requirements
Admin should be able to:
- upload or assign an image
- apply an image to multiple selected wines
- replace placeholder or poor images
- curate wines sharing the same producer/château efficiently

If full upload/storage is not yet ideal, document current assumptions clearly.

---

## Direct pairing management

### Goal
Allow Admin to assign meals to selected wines.

### Requirements
Admin should be able to:
- select multiple wines
- assign one or more meals
- create direct pairings in bulk
- avoid duplicate pairing creation

This page manages direct pairings, not pairing rules.

Pairing rules belong in the dedicated admin pairing rules page.

---

## Single wine editing

### Goal
Support quick one-off corrections.

### Requirements
Admin should be able to:
- open one wine
- edit important fields
- manage image
- manage direct pairings

This can be simple.

---

## Indicators

The wine list should clearly show useful curation indicators such as:
- has image / missing image
- has direct pairing / missing pairing
- source/import source if relevant

---

## Backend expectations

- admin-only protection required
- list queries should support relevant filtering/sorting
- bulk actions should be validated safely
- duplicate direct pairings should be avoided
- success/error responses should be clear

---

## Definition of Done

This feature is successful when:
- Admin has a dedicated wine backoffice page
- Admin can filter and sort wines
- Admin can select multiple wines
- Admin can manage images in bulk
- Admin can assign direct pairings in bulk
- Admin can identify incomplete wine records
- admin-only access is enforced correctly