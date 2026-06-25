import { getDrinkStatus } from './drinkStatus'

const COURSE_TYPES = [
  { type: 'Starter',  label: 'Starter'     },
  { type: 'Main',     label: 'Main Course'  },
  { type: 'Dessert',  label: 'Dessert'      },
]

// Preferred wine colors per course — used as fallback when no pairing wine is in cellar
const COLOR_PREFS = {
  Starter: ['White', 'Sparkling', 'Rosé', 'Orange'],
  Main:    ['Red', 'White', 'Rosé'],
  Dessert: ['White', 'Sparkling', 'Fortified', 'Rosé'],
}

// Lower number = preferred first
const READINESS_ORDER = { 'Ready': 0, 'Drink Soon': 1, 'Past Peak': 3, 'Too Young': 4 }

function sortByReadiness(wines) {
  return [...wines].sort((a, b) => {
    const sa = getDrinkStatus(a)?.label
    const sb = getDrinkStatus(b)?.label
    return (READINESS_ORDER[sa] ?? 2) - (READINESS_ORDER[sb] ?? 2)
  })
}

function pickWithSeed(arr, seed, offset) {
  if (arr.length === 0) return null
  return arr[(seed + offset) % arr.length]
}

/**
 * Returns the ISO week number (1–53) for the current date in the format YYYY * 100 + week.
 * Stable for the entire week (Mon–Sun), changes every Monday.
 */
export function getWeekSeed() {
  const now = new Date()
  const startOfYear = new Date(now.getFullYear(), 0, 1)
  const weekNum = Math.ceil(((now - startOfYear) / 86400000 + startOfYear.getDay() + 1) / 7)
  return now.getFullYear() * 100 + weekNum
}

/**
 * Returns a human-readable label for the Monday of the current week.
 */
export function getWeekLabel() {
  const now = new Date()
  const day = now.getDay() || 7  // 1=Mon … 7=Sun
  const monday = new Date(now)
  monday.setDate(now.getDate() - day + 1)
  return monday.toLocaleDateString(undefined, { month: 'long', day: 'numeric', year: 'numeric' })
}

/**
 * Compose a 3-course menu from the user's cellar and available recipes.
 *
 * Wine selection priority per course:
 *   1. Direct pairing wine explicitly linked to the chosen recipe that is in the cellar
 *   2. Rule-based pairing wine matched via admin pairing rules (ready-to-drink first)
 *   3. Wine matching the color preference for the course (ready-to-drink first)
 *   4. Any available cellar wine (ready-to-drink first)
 *   5. Reuse a cellar wine if the cellar has fewer bottles than courses
 *
 * The `seed` is typically the week number — same seed → same menu all week.
 *
 * @param {{ wine: object }[]} cellarItems
 * @param {object[]} recipes
 * @param {number} seed
 * @param {{ wineId: number, recipeId: number, priority: number }[]} ruleCandidates
 * @returns {{ type: string, label: string, wine: object|null, recipe: object|null }[]}
 */
export function composeMenu(cellarItems, recipes, seed, ruleCandidates = []) {
  const sortedWines = sortByReadiness(cellarItems.map(i => i.wine))
  const usedWineIds = new Set()

  // Build lookup: recipeId → Set of wineIds matched by rules
  const ruleWinesByRecipe = new Map()
  for (const c of ruleCandidates) {
    if (!ruleWinesByRecipe.has(c.recipeId)) ruleWinesByRecipe.set(c.recipeId, new Set())
    ruleWinesByRecipe.get(c.recipeId).add(c.wineId)
  }

  return COURSE_TYPES.map((course, i) => {
    // 1. Pick a recipe of this course type
    const recipesOfType = recipes.filter(r => r.recipeType === course.type)
    const recipe = pickWithSeed(recipesOfType, seed, i)

    let wine = null

    // 2. Prefer a direct pairing wine from the recipe that is in the cellar
    if (recipe?.pairings?.length > 0) {
      const pairingIds = new Set(recipe.pairings.map(p => p.wineId))
      const paired = sortedWines.filter(w => pairingIds.has(w.id) && !usedWineIds.has(w.id))
      wine = paired[0] ?? null
    }

    // 3. Rule-based pairing — cellar wines matched by admin rules for this recipe
    if (!wine && recipe && ruleWinesByRecipe.has(recipe.id)) {
      const ruleIds = ruleWinesByRecipe.get(recipe.id)
      const ruleWines = sortedWines.filter(w => ruleIds.has(w.id) && !usedWineIds.has(w.id))
      wine = ruleWines[0] ?? null
    }

    // 4. Fall back to color preference (ready-to-drink first)
    if (!wine) {
      for (const color of (COLOR_PREFS[course.type] ?? [])) {
        const matching = sortedWines.filter(w => w.color === color && !usedWineIds.has(w.id))
        if (matching.length > 0) {
          wine = pickWithSeed(matching, seed, i)
          break
        }
      }
    }

    // 5. Fall back to any available cellar wine
    if (!wine) {
      const available = sortedWines.filter(w => !usedWineIds.has(w.id))
      wine = pickWithSeed(available, seed, i)
    }

    // 6. Reuse if cellar is very small (fewer wines than courses)
    if (!wine && sortedWines.length > 0) {
      wine = pickWithSeed(sortedWines, seed, i)
    }

    if (wine) usedWineIds.add(wine.id)

    return { type: course.type, label: course.label, wine, recipe: recipe ?? null }
  })
}
