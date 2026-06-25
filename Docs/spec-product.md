# Wine Cellar App — Product Specification

## Goal

Build a wine cellar web application that helps users:
- manage their personal cellar
- understand which bottles are ready to drink
- find meals that pair with their wines
- discover curated wine-and-meal experiences

The product should feel more like a premium consumer wine product than a technical inventory tool.

---

## Main user-facing features

The app includes these main feature areas:
- authentication
- wine catalog
- personal cellar
- meals catalog
- wine/meal pairings
- weekly menu
- admin wine curation
- admin pairing rules

---

## User roles

### Authenticated user
Authenticated users can:
- browse wines
- browse meals
- manage their own cellar
- create wines if still allowed by current MVP rules
- create meals if still allowed by current MVP rules
- view pairings
- view weekly menu if available
- record tasting history if supported by the current implementation

### Admin user
Admin users can additionally:
- access the wine backoffice
- manage wine images and metadata
- bulk curate wines
- manage direct pairings
- manage pairing rules

---

## Core product principles

- MVP first
- simple and maintainable implementation
- preserve working behavior where practical
- avoid overengineering
- prefer clear structured data over magic behavior
- prioritize user value on important flows
- prioritize usability and UI consistency
- keep the app understandable for both users and admins

---

## Domain overview

Main entities currently or conceptually include:
- User
- Cellar
- CellarItem
- Wine
- WineCepage
- Meal / Recipe
- RecipeWinePairing
- Tasting history / tasting note records if implemented
- Pairing rules if implemented

### Shared domain rules
- one user has exactly one cellar
- cellar items track bottle counts per wine
- if a bottle count reaches zero, the cellar item is removed
- wines are shared across users
- meals are shared across users unless a future feature changes this
- wines and meals can pair many-to-many
- pairings can come from direct links and, if implemented, pairing rules

---

## MVP scope

### In scope
- register/login
- wine catalog browsing
- wine creation/editing where allowed
- personal cellar management
- meal browsing
- meal creation/editing where allowed
- wine/meal pairing visibility
- tasting history on cellar if implemented
- admin wine curation tools
- admin pairing rule tools
- weekly menu experience if implemented
- search and filtering
- tests
- maintainable codebase

### Out of scope unless explicitly requested
- social/community features
- marketplace/e-commerce
- AI-heavy recommendation engine
- barcode scanning
- public profiles
- advanced collaboration workflows
- complex CMS features
- advanced audit/versioning systems

---

## Cross-feature expectations

- search/filter behavior should feel consistent across pages
- direct actions should be clear and easy to find
- pages should handle empty states gracefully
- wine data should be rich enough to support search, pairing, and cellar usefulness
- the app should remain usable even when some metadata is incomplete
- admin tools should support data curation without turning the whole app into an admin-first product

---

## Relationship between features

### Wine catalog
Source of shared wines available across the app.

### Cellar
Tracks what the user actually owns and should drink.

### Meals
Provides meal content that can be paired with wines.

### Pairings
Connect wines and meals through direct links and optionally rules.

### Weekly Menu
Uses cellar + pairings to propose a curated meal flow.

### Admin tools
Improve wine data quality, pairing quality, and long-term catalog usefulness.

---

## Definition of Done

The product direction is being followed when:
- each major feature has a clear dedicated spec
- the app remains maintainable
- user-facing flows are coherent
- admin flows remain practical
- search, pairing, and cellar experiences work together
- UI decisions support a premium wine/lifestyle direction