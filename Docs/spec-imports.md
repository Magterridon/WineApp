# Wine Cellar App — Imports and Seed Data Specification

## Goal

Define how imported wine datasets and curated seed data should support the app.

This includes:
- one-time wine imports
- seed catalog enrichment
- practical transformation rules
- data quality expectations

---

## Product role of imported data

Imported data exists to:
- enrich the shared wine catalog
- improve search usefulness
- improve cellar usefulness
- improve pairing and weekly-menu potential
- reduce the need for users to create every wine manually

---

## Current known import: Bordeaux wines

### Source
- file: `Data/BordeauxWines.csv`

### Scope
- Bordeaux region only
- vintages approximately 2000–2018
- imported as shared catalog wines

### Current import intent
- import useful wine identity metadata
- ignore low-value wide descriptor columns for now
- map score to simplified rank
- derive practical drinking-window guidance

---

## Import expectations

### Practicality
Imports should prioritize fields that improve the actual product, such as:
- name
- domain / producer
- year
- region
- appellation
- color
- rank/classification proxy
- drink window proxy where necessary

### Non-goal
Do not import huge volumes of low-value raw data just because it exists.

---

## Transformation rules

Transformation logic may include:
- encoding fixes
- name normalization
- producer/domain mapping
- appellation extraction
- color detection
- score-to-rank mapping
- drink-window heuristic mapping

These rules should be documented clearly for each import process.

---

## Deduplication

Imports should avoid creating duplicate wines.

Current expected uniqueness rule:
- `Name + Domain + Year`

If needed, the importer may use:
- in-memory deduplication
- database conflict protection
- normalization before comparison

---

## Data quality principles

- imported data should remain understandable
- approximated fields should be documented
- missing fields should not block import if the core wine record is still useful
- imported data should remain compatible with search, cellar, and pairing features

---

## Seed data

The app should include practical development/test seed data for:
- wines
- meals
- pairings where useful

Seed data should help local setup and feature testing.

---

## Definition of Done

This area is successful when:
- imported data improves the catalog
- transformation logic is documented
- duplicate creation is controlled
- imported wines remain usable in the main product
- seed data helps development and testing