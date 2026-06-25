export const COLOR_STYLES = {
  Red:       { bg: '#8B1A1A', text: 'white' },
  White:     { bg: '#c8a951', text: '#333' },
  'Rosé':    { bg: '#e8a0a0', text: '#333' },
  Sparkling: { bg: '#b0c4de', text: '#333' },
  Fortified: { bg: '#6B3A2A', text: 'white' },
  Orange:    { bg: '#c87941', text: 'white' },
}

/**
 * Returns an inline-style string for a wine color badge.
 * Returns an empty string if the color is unknown or absent.
 */
export function getWineColorStyle(color) {
  const s = COLOR_STYLES[color]
  return s ? `background-color:${s.bg};color:${s.text}` : ''
}
