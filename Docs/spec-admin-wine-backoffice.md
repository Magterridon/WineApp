# Wine Cellar App — Admin Pairing Rules Specification

## Goal

Provide an admin-only interface for creating and managing reusable wine-to-meal pairing rules.

This feature should allow Admin to define modular pairing logic without manually pairing every wine one by one.

---

## Core concept

A pairing rule matches wines using wine attributes, then links them to one or more meals.

Examples:
- all red Bordeaux wines pair with lamb and beef
- Pinot Noir pairs with duck
- Château Margaux wines pair with more premium dishes

This system complements direct pairings rather than replacing them.

---

## Access control

- only Admin users can access this page
- backend protection is required
- non-admin users must not access rule management

---

## Main objectives

Admin should be able to:
- create a rule
- edit a rule
- enable/disable a rule
- define one or more wine-matching conditions
- assign one or more meals as targets
- manage broad and specific rules together
- understand rule priority/specificity

---

## Rule structure

A rule should conceptually include:
- name
- active/inactive status
- optional description
- one or more conditions
- one or more target meals
- priority or specificity information if needed

---

## Recommended MVP condition fields

Support practical structured wine conditions such as:
- producer / domain / château
- wine name if useful
- appellation
- region
- country
- color / type
- cépage
- classification / rank
- wine style if supported

Use current schema where possible.

---

## Recommended MVP operators

Keep rule logic simple.

Recommended operators:
- equals
- contains
- one of / in list

Do not build a complex boolean rules engine in the first version.

---

## Rule targets

A single rule may target multiple meals.

This is required because:
- one wine can match multiple meals
- pairing is not one-to-one

---

## Specificity and priority

### Goal
Ensure deterministic behavior when multiple rules match.

### Recommended pairing resolution relevance
1. direct wine-meal pairing
2. producer/château-specific rule
3. appellation-specific rule
4. region/color/cépage rule
5. generic style rule

If exact rule-level hierarchy is difficult to encode, use:
- explicit priority
- and/or specificity derived from matched conditions

Behavior must remain deterministic.

---

## Admin UI expectations

A practical form-based interface is enough.

The page should allow Admin to:
- list rules
- inspect rule summary
- create/edit rule
- activate/deactivate rule
- see target meals
- understand whether a rule is broad or specific

Do not overengineer a visual no-code builder in the first version.

---

## Backend expectations

- rules must validate correctly
- inactive rules must not be evaluated
- duplicate rule-target links should be avoided
- invalid meal references should be prevented
- evaluation behavior should be predictable

---

## Relationship with direct pairings

Direct pairings and rule-based pairings must coexist.

The app should support:
- specific hand-curated pairings
- broad reusable rules
- future weekly-menu selection logic

---

## Definition of Done

This feature is successful when:
- Admin can create and manage pairing rules
- rules can target multiple meals
- rules can match wines through structured conditions
- rule behavior is deterministic
- direct pairings and rules can coexist
- the feature is compatible with Weekly Menu and future pairing suggestions