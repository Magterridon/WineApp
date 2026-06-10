# Wine Cellar App — Admin Wine Backoffice Specification

## Goal

Create a lightweight admin backoffice for managing and curating the shared wine catalog.

This backoffice should help Admin users:
- search and filter wines efficiently
- edit wine data
- bulk update wine metadata
- bulk assign or replace wine images
- bulk create or assign pairing data
- improve imported wine quality over time

This is an internal tool for data curation, not a public-facing user feature.

---

## Main objectives

The admin backoffice should make it easy to:
1. find groups of wines quickly
2. select multiple wines at once
3. apply bulk actions safely
4. improve catalog quality without manual one-by-one editing

---

## Access control

- only Admin users can access this backoffice
- all admin actions must be protected on the backend
- non-admin users must not see or access these pages/actions

---

## Scope

This first version focuses on wine catalog curation.

It should support:
- wine listing
- wine filtering
- wine selection
- bulk updates
- bulk image update
- bulk pairing assignment

This first version does not need:
- advanced audit logs
- multi-admin workflows
- approval chains
- version history
- complex asset management
- AI automation

---

## 1. Admin Wine List Page

### Goal
Provide a searchable and filterable list of wines for curation.

### Requirements
The admin should be able to:
- see a list/table of wines
- search wines
- filter wines using wine-related fields
- sort wines
- select one or many wines

### Search/filter fields
Support as many of the existing useful wine fields as possible, especially:
- wine name
- château / castle
- domaine / producer
- cuvée
- vintage/year
- country
- region
- appellation
- classification/rank
- color/type
- grape varieties / cépages
- drink status / drink window
- source/import source if available
- image presence
- pairing presence

### Notes
- use the existing schema as source of truth
- if some fields do not exist yet, only add them if truly needed and useful
- "castle" and "domaine" may map to producer/domain depending on current schema
- filtering should be practical, not overcomplicated

---

## 2. Sorting

### Goal
Help Admin quickly organize and review large wine lists.

### Requirements
Support sorting where practical by:
- wine name
- producer / château / domaine
- vintage
- appellation
- classification/rank
- region
- score if available
- image presence
- updated date if available

---

## 3. Selection and Bulk Actions

### Goal
Allow Admin to select multiple wines and apply updates efficiently.

### Requirements
The admin should be able to:
- select individual wines
- select multiple wines
- select all results on the current page
- ideally keep selection while filters are active

### Bulk actions for first version
Support bulk actions for:
- bulk update image
- bulk assign pairing
- bulk update selected metadata fields where practical

---

## 4. Bulk Image Update

### Goal
Allow Admin to replace placeholder/random images with better wine images for groups of wines.

### Requirements
The admin should be able to:
- upload an image file
- apply that image to multiple selected wines
- replace existing placeholder images
- update many wines sharing the same château/domaine/producer with one action

### Example use case
- select all wines from Château Margaux
- upload one image
- apply that image to all selected wines

### Notes
- if file upload already exists in the project, reuse it
- if not, implement the simplest maintainable upload/storage flow already compatible with the current stack
- document storage assumptions
- if full upload infrastructure is too large, a fallback image URL input may also be supported, but upload is preferred

---

## 5. Bulk Pairing Assignment

### Goal
Allow Admin to assign pairing information to multiple wines at once.

### Requirements
The admin should be able to:
- select multiple wines
- assign one or more recipes/meals to them
- create pairing associations in bulk
- avoid duplicate pairing creation if the relation already exists

### Pairing scope
Bulk pairing may be applied:
- directly to selected wines
- based on common producer/appellation/style if supported by the current model

### Notes
- keep this consistent with the current pairing model
- do not redesign pairing architecture unless necessary
- make only minimal schema/API changes

---

## 6. Single Wine Edit Support

### Goal
Allow Admin to quickly fix one wine when bulk action is not appropriate.

### Requirements
From the admin wine list, Admin should be able to:
- open a single wine edit view/page/modal
- update key wine fields
- change image
- manage pairings for one wine

This can be simple in the first version.

---

## 7. Image / Pairing Indicators

### Goal
Help Admin identify incomplete records quickly.

### Requirements
In the admin wine list, show clear indicators for:
- has image / missing image
- has pairing / missing pairing
- source/import source if relevant

This helps prioritize curation work.

---

## 8. API / Backend Expectations

### Requirements
- add admin-only endpoints or protect existing endpoints appropriately
- support filtering/sorting for admin list queries
- support bulk update actions safely
- support image upload and bulk assignment
- support bulk pairing assignment
- validate admin permissions on the backend

### Data integrity
- avoid duplicate pairings
- avoid invalid bulk updates
- return clear success/error responses

---

## 9. UI / UX Expectations

### Requirements
- keep the admin interface practical and fast
- use table/list layout if appropriate
- allow filtering without clutter
- make selected item count visible
- make bulk actions obvious
- confirm destructive or large actions if needed
- show success/error feedback clearly

### Key usability requirement
This page should make it easy to do things like:
- filter all wines from Château Margaux
- select them all
- upload and assign a better image
- filter wines with missing pairings
- assign one recipe to many wines at once

---

## 10. Technical Constraints

- review the current implementation first
- preserve the current schema as much as possible
- make only minimal schema/API/UI changes
- do not overengineer a full CMS
- keep the implementation maintainable
- preserve existing wine pages and user flows
- update tests where needed

---

## 11. Definition of Done

This work is successful when:
- Admin has a dedicated wine backoffice page
- Admin can filter and sort wines using useful wine fields
- Admin can select multiple wines
- Admin can bulk upload/apply an image to selected wines
- Admin can bulk assign pairings to selected wines
- Admin can identify wines missing images or pairings
- Admin-only access is enforced properly
- the tool is practical for ongoing catalog curation