# Wine Cellar App — Strong MVP Functional Specification

## Goal

Strengthen the MVP so it delivers clear user value for the main use case:

- manage a personal wine cellar
- know which bottles should be drunk soon
- know with what to drink them
- easily browse and search wines without manually creating everything

This phase should improve the product value and usability without overengineering.

---

## Product Priorities

The app should become strong in these 4 areas:

1. wine catalog quality
2. cellar usability
3. drinking window guidance
4. food pairing usefulness

---

## Scope

This phase focuses on:
- better search and filtering
- richer wine metadata
- clearer drinking guidance
- better pairing visibility
- better cellar insights
- duplicate prevention
- admin catalog curation UX
- UI/UX polish for main user flows

This phase does not focus on:
- social features
- marketplace/e-commerce
- barcode scanning
- recommendation AI
- advanced statistics
- public profiles

---

## 1. Search and Filtering

### Goal
Help users quickly find wines in the catalog and in their cellar.

### Requirements

Wine search/filter should support:
- name
- producer/domain
- cuvée
- vintage
- country
- region
- appellation
- color/type
- grape/cepage
- drink window status

Cellar filter should support:
- all bottles
- ready to drink
- drink soon
- past peak
- by region/appellation
- by color/type

Recipe search/filter should support:
- recipe name
- ingredient
- type/category
- linked wine/appellation
- pairing compatibility

---

## 2. Improved Wine Data Model

### Goal
Store enough structured information to make wines searchable and useful.

### Wine fields should support
- name
- producer/domain
- cuvée
- vintage
- country
- region
- appellation
- classification if available
- color/type
- description
- imageUrl
- drinkFromYear
- drinkToYear
- grape varieties / cepages
- source / external reference if imported

### Notes
- keep the model simple
- prefer structured fields over long free text where possible
- keep backward compatibility if possible

---

## 3. Drinking Window Guidance

### Goal
Make it obvious when a bottle should be consumed.

### Requirements
Each wine should display a drinking status derived from available data:
- Too young
- Ready to drink
- Peak
- Drink soon
- Past peak

### UI Requirements
Display this status:
- on wine detail page
- on cellar page
- in search/filter options
- in bottle summary cards or rows if possible

### Logic
Use `drinkFromYear` and `drinkToYear` as the base.
If richer logic is not available, use a simple, understandable rule based on the current year.

---

## 4. Pairing Visibility and Usefulness

### Goal
Help users decide what to eat with a wine.

### Requirements
Wine detail pages should show:
- linked recipes
- pairing notes
- meal categories/tags if available

Recipe detail pages should show:
- linked wines
- linked appellations or wine styles if modeled
- pairing notes

### User drink history should support
- wine rating
- tasting note
- meal used
- pairing rating
- pairing note

### Future-safe rule
Keep pairing data structured enough so it can later support broader matching by:
- appellation
- color/type
- grape
- style

---

## 5. Cellar Insights

### Goal
Make the cellar page immediately useful.

### Requirements
The cellar page should clearly highlight:
- bottles ready to drink
- bottles to drink soon
- bottles past peak
- recently drunk wines
- current bottle count

### Nice-to-have
- simple summary cards at top of cellar page:
  - total bottles
  - ready to drink
  - drink soon
  - past peak

---

## 6. Duplicate Prevention

### Goal
Avoid a messy wine catalog.

### Requirements
Before creating a new wine:
- search existing catalog first
- show possible matches
- encourage reuse of existing records

### Notes
- this can be simple suggestion logic at first
- no need for advanced fuzzy matching in first version

---

## 7. Admin Curation

### Goal
Let Admin keep catalog and pairing quality high.

### Admin should be able to
- create/edit/delete wines
- create/edit/delete recipes
- create/edit/delete pairings
- enrich wine data
- maintain drink window information

### User should be able to
- browse wines and recipes
- manage personal cellar
- use drink history
- create new wines for now, if still allowed by current rules

---

## 8. UI / UX Polish

### Goal
Make the main flows clean and pleasant.

### Requirements
Improve:
- loading states
- empty states
- flash/success/error messages
- form clarity
- mobile responsiveness
- table/card readability
- action visibility
- button wording consistency

### Key pages to polish
- cellar page
- wine details page
- wines page
- recipes page
- drink flow/modal/page

---

## 9. Definition of Done

This phase is successful when:
- users can find wines more easily
- the cellar clearly shows what to drink soon
- wine pages better explain pairing and drink timing
- duplicate wine creation is reduced
- admin can curate the catalog and pairings more effectively
- the main flows feel clear and polished