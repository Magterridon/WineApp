# Wine Cellar App — Pairings Specification

## Goal

Define how wines and meals are connected across the app.

The app should support many-to-many relationships between wines and meals, so that:
- one wine can pair with many meals
- one meal can pair with many wines

This pairing system should support both direct curated pairings and future/admin-managed pairing rules.

---

## Pairing sources

### 1. Direct pairings
A direct pairing explicitly links:
- one wine
- one meal

This is useful for:
- hand-curated pairings
- specific featured matches
- simple current MVP behavior

### 2. Pairing rules
If implemented, pairing rules allow Admin to define reusable pairing logic based on wine attributes.

Examples:
- Bordeaux red wines pair with lamb and beef meals
- Pinot Noir pairs with duck meals
- Château-specific wines pair with more premium dishes

---

## Core rules

- pairings are many-to-many
- direct pairings remain valid and useful
- rule-based pairings extend the system
- the app should avoid duplicate visible results
- the pairing system should remain deterministic

---

## Pairing visibility across the app

### Wine detail
Should show meals that pair with the wine.

### Meal detail
Should show wines that pair with the meal.

### Wine catalog / cellar
May expose pairing-aware filtering/search where useful.

### Weekly Menu
May use pairings to select meals and wines together.

---

## Pairing data expectations

A pairing may include:
- wine reference
- meal reference
- optional notes
- optional source/type if needed later
- optional priority metadata if needed later

Only add fields that are actually useful.

---

## Coexistence of direct pairings and rules

If both direct pairings and rules exist:
- direct pairings should have higher relevance
- more specific rules should outrank broad generic rules
- the app should merge results cleanly
- the user experience should stay simple even if the underlying system is richer

---

## Pairing management

### Standard product behavior
Users mainly consume pairing information.

### Admin behavior
Admin can:
- manage direct pairings
- manage pairing rules
- curate better pairing quality over time

---

## Weekly Menu compatibility

The pairing system should support future or existing weekly menu behavior, where the app selects:
- multiple meals
- multiple wines
- from valid pairing candidates

The first version does not need a heavy recommendation engine, but should not block one.

---

## Definition of Done

This feature is successful when:
- wines and meals can pair many-to-many
- direct pairings work clearly
- pairing visibility works on wine and meal pages
- the system stays compatible with future rule-based pairing logic