# Bordeaux Wine Import

## Overview

This document describes the one-time import of Bordeaux wine data from the file `Data/BordeauxWines.csv`
into the WineApp PostgreSQL database.

---

## Dataset

| Field        | Value                              |
|--------------|------------------------------------|
| File         | `Data/BordeauxWines.csv`           |
| Rows         | 14,349 data rows                   |
| Total columns| 989                                 |
| Source scope | Bordeaux region only               |
| Vintages     | 2000–2018                          |
| Scores       | 60–100 (avg ≈ 87.8)               |

---

## Column selection

| Column(s)    | Action   | Reason                                       |
|--------------|----------|----------------------------------------------|
| Name (col 1) | Imported | Wine identity (after encoding fix + parsing) |
| Year (col 2) | Imported | Vintage year                                 |
| Score (col 3)| Used     | Mapped to Rank (1-5); not stored raw         |
| Price (col 4)| Ignored  | Not used by the current app UI               |
| Cols 5–989   | Ignored  | Binary aroma descriptor matrix — no product value (see spec-data-integration.md §low-priority) |

---

## How to run

From the `backend/WineApp.Importer` directory:

```bash
# Default (localhost:5432, database=wineapp, user=postgres, password=postgres)
dotnet run -- --file ../../Data/BordeauxWines.csv

# Custom connection
dotnet run -- --file ../../Data/BordeauxWines.csv --connection "Host=myhost;Port=5432;Database=wineapp;Username=me;Password=s3cr3t"

# Dry run (nothing written, first 10 rows previewed)
dotnet run -- --file ../../Data/BordeauxWines.csv --dry-run

# Filter to only high-scoring wines
dotnet run -- --file ../../Data/BordeauxWines.csv --min-score 90

# Or use DATABASE_URL environment variable
$env:DATABASE_URL = "Host=..."; dotnet run -- --file ../../Data/BordeauxWines.csv
```

The tool is idempotent — re-running it will skip any wine already present in the database
(matched by Name + Domain + Year).

---

## Transformation logic

### Encoding fix

The source CSV is valid UTF-8 but contains Mojibake — French characters were originally Latin-1
and were incorrectly re-encoded as UTF-8. Each character is converted back:

```
raw bytes as Latin-1 → re-decoded as UTF-8
"ChÃ¢teau" → "Château"
```

### Name / appellation splitting

The `Name` column concatenates the château name and the AOC appellation, e.g.:

```
"Château Croix Figeac St.-Emilion"  →  name="Château Croix Figeac"  appellation="St.-Emilion"
"Château Smith-Haut-Lafitte Pessac-Léognan"  →  name="..."  appellation="Pessac-Léognan"
```

Matching uses an ordered list of 29 known Bordeaux AOC names (longest first to prevent partial
matches). If no pattern matches, appellation defaults to "Bordeaux".

### Color detection

Wines containing " White" (case-insensitive) in their name are tagged `Color = "White"`.
All other wines default to `Color = "Red"`.
The keyword is stripped before name/appellation parsing.

### Score → Rank mapping

| Score range | Rank |
|-------------|------|
| 95–100      | ★★★★★ (5) |
| 90–94       | ★★★★ (4)  |
| 85–89       | ★★★ (3)   |
| 80–84       | ★★ (2)    |
| < 80        | ★ (1)     |

### Drinking window heuristic

Because the dataset provides no precise drinking-window data, a simple conservative heuristic
is applied based on score tier (per `spec-data-integration.md`):

| Score  | DrinkFromYear    | DrinkToYear      | Window |
|--------|------------------|------------------|--------|
| ≥ 90   | vintage + 5 yrs  | vintage + 25 yrs | 20 yr  |
| < 90   | vintage + 2 yrs  | vintage + 12 yrs | 10 yr  |

**These are rough guidelines, not expert assessments.** They are intended to help users
decide when to open bottles, not to replace professional cellaring advice.

### Fixed fields

All imported Bordeaux wines receive:

| Field   | Value      |
|---------|------------|
| Country | France     |
| Region  | Bordeaux   |
| Domain  | (same as Name — château is the producer) |

---

## Deduplication

The tool maintains an in-memory set of `Name|Domain|Year` keys loaded from the database
before the import starts. Any row whose key already exists is counted as skipped and not
re-inserted. The SQL also includes `ON CONFLICT DO NOTHING` as a safety net.

---

## Import results (initial run — 2026-06-09)

| Metric                    | Count  |
|---------------------------|--------|
| Total CSV rows            | 14,349 |
| Inserted                  | 11,379 |
| Skipped (duplicates)      | 2,970  |
| Filtered (score threshold)| 0      |
| Errors                    | 0      |

Post-import database state (`Region = 'Bordeaux'`):

| Metric           | Value  |
|------------------|--------|
| Total wines      | 11,380 |
| Appellations     | 29     |
| Red wines        | 10,865 |
| White wines      | 515    |
| Vintage range    | 2000–2018 |
| Rank range       | 1–5    |

The 2,970 duplicates arose from wines that appeared under multiple appellations in the source
file but parsed to the same château name + vintage (e.g., a wine listed under both its grand
cru and regional AOC).

---

## How to verify

```sql
-- Row counts by appellation
SELECT "Appellation", COUNT(*) AS wines
FROM "Wines"
WHERE "Region" = 'Bordeaux'
GROUP BY "Appellation"
ORDER BY wines DESC;

-- Rank distribution
SELECT "Rank", COUNT(*) FROM "Wines" WHERE "Region" = 'Bordeaux' GROUP BY "Rank" ORDER BY "Rank";

-- Sample: a famous Pauillac
SELECT "Name", "Year", "Rank", "DrinkFromYear", "DrinkToYear", "Color", "Appellation"
FROM "Wines"
WHERE "Name" LIKE 'Château Latour%' AND "Region" = 'Bordeaux'
ORDER BY "Year";

-- White wines sample
SELECT "Name", "Year", "Appellation"
FROM "Wines"
WHERE "Region" = 'Bordeaux' AND "Color" = 'White'
LIMIT 10;
```

---

## Known limitations

- **No score stored**: the original 100-point score is mapped to a 1–5 rank and the raw value
  is discarded. Fine for the current UI; store if score display is added later.
- **Drinking window is approximate**: the heuristic (score ≥ 90 → 20-yr window, else 10-yr)
  is a rough guide. Premium wines like Pétrus or Haut-Brion from great vintages can age 30–50 years.
- **Domain = Name**: for Bordeaux, the château IS the producer. If the app later distinguishes
  château names from domain/négociant names, this will need revisiting.
- **White wine detection by keyword**: works for this dataset (all whites are named "X White"),
  but would not generalise to other datasets without adjustment.
- **Price not stored**: column 4 (price, e.g. `$45`) was ignored since the app has no price
  display yet.
- **Cépages not populated**: the dataset has no grape-variety column; `WineCepage` rows are
  not created by this import.
- **Year range**: dataset covers 2000–2018 only. Add newer vintages by re-importing or
  manually adding wines via the app.
