# Wine Cellar App — Data Integration Specification

## Goal

Define how external wine datasets should be reviewed, cleaned, mapped, and imported into the project database.

This specification exists to ensure that future imports:
- stay aligned with the current product model
- do not pollute the database with low-value fields
- remain maintainable
- preserve data quality
- support the MVP goal of helping users manage their cellar and know when and with what to drink their wines

---

## Core principle

External datasets must be adapted to the product model.

The project schema should not be redesigned around each dataset.

Imports must be selective, intentional, and product-driven.

---

## Import priorities

When importing external wine data, prioritize fields that improve the current app experience, especially:
- wine identity
- searchability
- display quality
- drinking window guidance
- pairing usefulness where relevant

Low-value or highly technical columns should be ignored unless there is a clear current product use for them.

---

## Default import workflow

For each new dataset, follow this process:

1. inspect the dataset structure
2. evaluate data quality
3. identify useful columns
4. map only useful columns to the current schema
5. clean and normalize data
6. deduplicate records
7. import with a reproducible script or command
8. document what was imported and what was ignored

---

## Schema policy

### Preserve the current schema first
- Review the existing schema before each import
- Reuse the existing model whenever possible
- Avoid schema changes unless truly needed

### Minimal schema changes only
If a dataset introduces a valuable field not supported by the current model:
- make only the minimal schema change required
- explain why the change is useful for the product
- avoid adding fields that are not yet used by the app

### Never import blindly
- Do not create database columns just because they exist in the dataset
- Do not mirror large external schemas directly into the core product model
- Do not add hundreds of descriptor columns to the main wine table

---

## Data selection rules

### High-priority fields
Import these when available and useful:
- wine name
- producer/domain
- cuvée
- vintage/year
- country
- region
- appellation
- classification
- color/type
- score/rating from the source
- price
- source name
- source external identifier
- useful drinking window fields if directly available

### Medium-priority fields
Import only if there is a clear current use:
- grape varieties
- short description
- image URL
- ranking/classification labels
- pairing-related structured data

### Low-priority fields
Usually ignore for MVP unless a specific feature requires them:
- massive aroma descriptor matrices
- large binary feature vectors
- review language fragments
- internal export-only columns
- redundant text fields
- derived ML features
- opaque flags without product value

---

## Cleaning and normalization rules

### Text normalization
- trim whitespace
- normalize capitalization where appropriate
- preserve correct wine naming
- fix encoding issues when detected
- example: `ChÃ¢teau` should become `Château`

### Numeric normalization
- parse score into a numeric field if useful
- normalize prices into a consistent format
- strip currency symbols if needed before storage
- handle missing/invalid numeric values safely

### Missing values
- allow partial import when non-critical fields are missing
- skip rows only when the record cannot be identified meaningfully
- log skipped rows when useful

---

## Region and source rules

### Dataset scope
If a dataset is region-specific:
- preserve that scope clearly
- assign region/country defaults only when justified by the dataset
- document any assumptions applied during import

### Source traceability
Each imported record should keep enough source traceability to understand:
- where it came from
- which import produced it
- what assumptions were applied

At minimum, keep:
- source name
- source file or source identifier if practical

---

## Drinking window / conservation guidance

### Principle
If imported datasets do not provide reliable drinking-window data, the importer may apply simple approximate rules.

### Allowed approach
- use a simple explainable heuristic
- prefer conservative defaults
- keep rules easy to understand and maintain

### Example
For Bordeaux imports:
- better-quality wines may use an approximate drink window of 20 to 30 years
- lower-quality wines may use an approximate drink window of 10 to 20 years

### Important constraints
- do not present heuristic values as precise expert truth
- document the rule applied
- avoid overfitted bottle-specific aging predictions unless reliable source data exists

---

## Deduplication rules

- avoid creating obvious duplicates
- use practical matching keys such as:
  - normalized wine name
  - vintage
  - producer/domain if available
  - source identifier if available
- keep deduplication simple and explainable
- prefer idempotent imports where reasonably possible

---

## Technical implementation rules

- every import should be runnable locally
- prefer a script/command that can be rerun
- add basic logging
- add basic error handling
- keep the implementation understandable
- do not break existing application behavior
- add migrations only if schema changes are required

---

## Documentation requirements

For each import implementation, document:
- which dataset was used
- which columns were imported
- which columns were ignored
- which assumptions were applied
- any schema changes
- how to run the import
- how to verify the results
- known limitations

---

## MVP decision rule

When unsure whether to import a field, ask:

Does this improve the current MVP for:
- managing a wine cellar
- knowing when to drink the bottle
- knowing with what to drink it
- searching and understanding wines

If not, do not import it into the main model yet.