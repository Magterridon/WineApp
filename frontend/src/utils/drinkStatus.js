export { COLOR_STYLES } from './wineColor.js'

const CURRENT_YEAR = new Date().getFullYear()

/**
 * Returns a drink status object for a wine, or null if no window data exists.
 * @param {{ drinkFromYear?: number, drinkToYear?: number }} wine
 * @returns {{ label: string, bg: string } | null}
 */
export function getDrinkStatus(wine) {
  const { drinkFromYear, drinkToYear } = wine
  if (!drinkFromYear && !drinkToYear) return null

  if (drinkToYear && CURRENT_YEAR > drinkToYear)
    return { label: 'Past Peak', bg: 'danger' }

  if (drinkFromYear && CURRENT_YEAR < drinkFromYear)
    return { label: 'Too Young', bg: 'secondary' }

  if (drinkToYear && drinkToYear - CURRENT_YEAR <= 2)
    return { label: 'Drink Soon', bg: 'warning' }

  return { label: 'Ready', bg: 'success' }
}

export const WINE_COLORS = ['Red', 'White', 'Rosé', 'Sparkling', 'Fortified', 'Orange']

export const DRINK_STATUS_OPTIONS = [
  { value: '',      label: 'All statuses' },
  { value: 'young', label: 'Too Young' },
  { value: 'ready', label: 'Ready' },
  { value: 'soon',  label: 'Drink Soon' },
  { value: 'past',  label: 'Past Peak' },
]
