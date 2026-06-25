import { describe, it, expect } from 'vitest'
import { composeMenu } from '@/utils/menuComposer'

// ── Helpers ─────────────────────────────────────────────────────────────────

function makeItem(wine) {
  return { wineId: wine.id, bottleCount: 1, wine }
}

// ── Sample wines ─────────────────────────────────────────────────────────────

const readyRed = {
  id: 1, name: 'Gevrey', domain: 'Rossignol', year: 2019,
  color: 'Red', drinkFromYear: 2023, drinkToYear: 2035, rank: 4, cepages: []
}

const soonWhite = {
  id: 2, name: 'Sancerre', domain: 'Bourgeois', year: 2021,
  color: 'White', drinkFromYear: 2022, drinkToYear: 2026, rank: 3, cepages: []
}

const youngRed = {
  id: 3, name: 'Young Bordeaux', domain: 'Château X', year: 2022,
  color: 'Red', drinkFromYear: 2030, drinkToYear: 2045, rank: 5, cepages: []
}

const sparkling = {
  id: 4, name: 'Champagne', domain: 'Moët', year: 2018,
  color: 'Sparkling', drinkFromYear: 2020, drinkToYear: 2030, rank: 4, cepages: []
}

// ── Sample recipes ───────────────────────────────────────────────────────────

const starterRecipe = {
  id: 10, name: 'Tartare de Saumon', recipeType: 'Starter',
  description: 'Saumon frais avec câpres.',
  ingredients: ['400g saumon', '2 échalotes', '1 citron'],
  instructions: 'Hacher, mélanger, dresser.',
  pairings: [{ wineId: 2 }]   // pairs with soonWhite
}

const mainRecipe = {
  id: 11, name: 'Boeuf Bourguignon', recipeType: 'Main',
  description: 'Classique bourguignon.',
  ingredients: ['1.5kg boeuf', '1 bouteille Bourgogne rouge'],
  instructions: 'Mariner, cuire 3h.',
  pairings: [{ wineId: 1 }]   // pairs with readyRed
}

const dessertRecipe = {
  id: 12, name: 'Tiramisu', recipeType: 'Dessert',
  description: 'Tiramisu classique.',
  ingredients: ['500g mascarpone', '4 oeufs', 'café fort'],
  instructions: 'Monter, tremper, alterner.',
  pairings: []   // no pairing defined
}

const anotherMain = {
  id: 13, name: 'Magret de Canard', recipeType: 'Main',
  description: 'Magret aux cerises.',
  ingredients: ['2 magrets', '300g cerises'],
  instructions: 'Cuire, déglacer.',
  pairings: []
}

// ── Tests ────────────────────────────────────────────────────────────────────

describe('composeMenu', () => {
  const allRecipes = [starterRecipe, mainRecipe, dessertRecipe]

  it('returns exactly 3 courses with correct types and labels', () => {
    const menu = composeMenu([makeItem(readyRed)], allRecipes, 100)
    expect(menu).toHaveLength(3)
    expect(menu.map(c => c.type)).toEqual(['Starter', 'Main', 'Dessert'])
    expect(menu.map(c => c.label)).toEqual(['Starter', 'Main Course', 'Dessert'])
  })

  it('assigns null wine for every course when cellar is empty', () => {
    const menu = composeMenu([], allRecipes, 100)
    expect(menu.every(c => c.wine === null)).toBe(true)
  })

  it('assigns null recipe for a course with no matching recipe type', () => {
    const menu = composeMenu([makeItem(readyRed)], [mainRecipe], 100)
    expect(menu.find(c => c.type === 'Starter')?.recipe).toBeNull()
    expect(menu.find(c => c.type === 'Dessert')?.recipe).toBeNull()
  })

  it('prefers pairing wine from cellar over color preference', () => {
    const items = [makeItem(readyRed), makeItem(soonWhite)]
    const menu = composeMenu(items, allRecipes, 100)
    // Starter pairs with soonWhite (id 2)
    expect(menu.find(c => c.type === 'Starter')?.wine?.id).toBe(2)
    // Main pairs with readyRed (id 1)
    expect(menu.find(c => c.type === 'Main')?.wine?.id).toBe(1)
  })

  it('falls back to color preference when pairing wine is not in cellar', () => {
    // Only sparkling in cellar; starter prefers White, Sparkling, Rosé, Orange
    const items = [makeItem(sparkling)]
    const menu = composeMenu(items, [starterRecipe], 100)
    const starter = menu.find(c => c.type === 'Starter')
    // soonWhite (the pairing wine) is not in cellar → fall back to color → Sparkling matches
    expect(starter?.wine?.id).toBe(4)
  })

  it('falls back to any available wine when no color preference matches', () => {
    // Only Red in cellar; starter prefers White/Sparkling/Rosé/Orange; pairing wine not in cellar
    const items = [makeItem(readyRed)]
    const menu = composeMenu(items, [starterRecipe], 100)
    const starter = menu.find(c => c.type === 'Starter')
    // No color match → any available → readyRed
    expect(starter?.wine?.id).toBe(1)
  })

  it('does not reuse the same wine across courses when cellar has enough wines', () => {
    const items = [makeItem(readyRed), makeItem(soonWhite), makeItem(sparkling)]
    const menu = composeMenu(items, allRecipes, 100)
    const wineIds = menu.filter(c => c.wine).map(c => c.wine.id)
    expect(new Set(wineIds).size).toBe(wineIds.length)
  })

  it('prefers ready-to-drink wines over too-young wines when same color', () => {
    // Both wines are White (Starter's top preference) but different readiness.
    // youngWhite is listed first in the input array.
    // After sortByReadiness, readyWhite should move to index 0.
    // seed=0, offset=0 (Starter is course 0): (0+0)%2=0 → picks index 0 → readyWhite wins.
    const readyWhite = {
      id: 5, name: 'Chablis', domain: 'Fèvre', year: 2021,
      color: 'White', drinkFromYear: 2022, drinkToYear: 2029, rank: 3, cepages: []
    }
    const youngWhite = {
      id: 6, name: 'Meursault Futur', domain: 'X', year: 2023,
      color: 'White', drinkFromYear: 2030, drinkToYear: 2045, rank: 4, cepages: []
    }
    const starterNoBinding = {
      id: 20, name: 'Salade', recipeType: 'Starter',
      description: '', ingredients: [], instructions: '', pairings: []
    }
    const items = [makeItem(youngWhite), makeItem(readyWhite)]  // youngWhite first in array
    const menu = composeMenu(items, [starterNoBinding], 0)      // seed 0 → index 0 from sorted list
    const starter = menu.find(c => c.type === 'Starter')
    expect(starter?.wine?.id).toBe(5)  // readyWhite (id 5), not youngWhite (id 6)
  })

  it('returns the same menu for the same seed (deterministic)', () => {
    const items = [makeItem(readyRed), makeItem(soonWhite)]
    const menu1 = composeMenu(items, allRecipes, 202625)
    const menu2 = composeMenu(items, allRecipes, 202625)
    expect(menu1.map(c => c.wine?.id)).toEqual(menu2.map(c => c.wine?.id))
    expect(menu1.map(c => c.recipe?.id)).toEqual(menu2.map(c => c.recipe?.id))
  })

  it('picks a different main course recipe for a different seed (when multiple mains exist)', () => {
    const items = [makeItem(readyRed), makeItem(soonWhite)]
    const recipes = [starterRecipe, mainRecipe, anotherMain, dessertRecipe]
    // With 2 main recipes and different seeds, the picked main should eventually differ
    const menus = Array.from({ length: 10 }, (_, n) => composeMenu(items, recipes, n))
    const mainIds = menus.map(m => m.find(c => c.type === 'Main')?.recipe?.id)
    // Both recipe ids should appear across 10 seeds
    expect(mainIds).toContain(mainRecipe.id)
    expect(mainIds).toContain(anotherMain.id)
  })

  it('handles a recipe with ingredients array gracefully', () => {
    const menu = composeMenu([makeItem(readyRed)], allRecipes, 100)
    const starter = menu.find(c => c.type === 'Starter')
    expect(Array.isArray(starter?.recipe?.ingredients)).toBe(true)
    expect(starter?.recipe?.ingredients.length).toBeGreaterThan(0)
  })
})

// ── Rule candidates ───────────────────────────────────────────────────────────

describe('composeMenu — rule candidates', () => {
  const ruleRed = {
    id: 50, name: 'Rule Red', domain: 'X', year: 2020,
    color: 'Red', drinkFromYear: 2022, drinkToYear: 2030, rank: 3, cepages: []
  }
  const ruleWhite = {
    id: 51, name: 'Rule White', domain: 'Y', year: 2021,
    color: 'White', drinkFromYear: 2022, drinkToYear: 2028, rank: 3, cepages: []
  }

  const mainNoDirectPairing = {
    id: 60, name: 'Lamb Tagine', recipeType: 'Main',
    description: '', ingredients: [], instructions: '', pairings: []
  }
  const starterNoDirectPairing = {
    id: 61, name: 'Bruschetta', recipeType: 'Starter',
    description: '', ingredients: [], instructions: '', pairings: []
  }

  it('uses rule candidate when no direct pairing matches', () => {
    const items = [makeItem(ruleRed)]
    const candidates = [{ wineId: 50, recipeId: 60, priority: 10, ruleName: 'Test' }]
    const menu = composeMenu(items, [mainNoDirectPairing], 0, candidates)
    expect(menu.find(c => c.type === 'Main')?.wine?.id).toBe(50)
  })

  it('direct pairing takes precedence over rule candidate', () => {
    // starterRecipe has direct pairing with soonWhite (id=2)
    // ruleRed is matched via rule for the same recipe
    const items = [makeItem(soonWhite), makeItem(ruleRed)]
    const candidates = [{ wineId: 50, recipeId: starterRecipe.id, priority: 20, ruleName: 'Rule' }]
    const menu = composeMenu(items, [starterRecipe], 100, candidates)
    expect(menu.find(c => c.type === 'Starter')?.wine?.id).toBe(2) // soonWhite via direct pairing
  })

  it('rule pairing takes precedence over color preference', () => {
    // Only ruleRed is in cellar; no direct pairing.
    // Rule matches ruleRed to mainNoDirectPairing.
    // Without rules, Main would fall through to color pref (Red matches), but the rule should be applied first.
    const sparkling2 = { id: 52, name: 'Ch2', domain: 'Z', year: 2020, color: 'Sparkling',
                         drinkFromYear: 2021, drinkToYear: 2030, rank: 3, cepages: [] }
    const items = [makeItem(sparkling2), makeItem(ruleRed)]
    const candidates = [{ wineId: 50, recipeId: mainNoDirectPairing.id, priority: 10, ruleName: 'Test' }]
    const menu = composeMenu(items, [mainNoDirectPairing], 0, candidates)
    // ruleRed (id=50) is picked via rule, NOT sparkling2 via color pref
    expect(menu.find(c => c.type === 'Main')?.wine?.id).toBe(50)
  })

  it('ignores rule candidates for wines not in cellar', () => {
    // Rule says wineId=99 pairs with mainNoDirectPairing, but that wine is not in cellar
    const items = [makeItem(ruleWhite)]
    const candidates = [{ wineId: 99, recipeId: mainNoDirectPairing.id, priority: 10, ruleName: 'Ghost' }]
    const menu = composeMenu(items, [mainNoDirectPairing], 0, candidates)
    // ruleWhite (51) in cellar; Main prefers Red/White/Rosé → falls back to color pref → ruleWhite
    expect(menu.find(c => c.type === 'Main')?.wine?.id).toBe(51)
  })

  it('behaves identically to no-rules call when ruleCandidates is omitted', () => {
    const items = [makeItem(readyRed), makeItem(soonWhite)]
    const withEmpty  = composeMenu(items, [starterRecipe, mainRecipe, dessertRecipe], 100, [])
    const withOmit   = composeMenu(items, [starterRecipe, mainRecipe, dessertRecipe], 100)
    expect(withEmpty.map(c => c.wine?.id)).toEqual(withOmit.map(c => c.wine?.id))
  })
})
