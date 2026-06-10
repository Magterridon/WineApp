import api from './api'

export const drinkService = {
  async drinkBottle(wineId, { rating, tastingNote, recipeId, mealNote, pairingRating, pairingNote, consumedAt }) {
    const { data } = await api.post('/api/cellar/drink', {
      wineId,
      rating: rating || null,
      tastingNote: tastingNote || null,
      recipeId: recipeId || null,
      mealNote: mealNote || null,
      pairingRating: pairingRating || null,
      pairingNote: pairingNote || null,
      consumedAt: consumedAt || null
    })
    return data
  },

  async getHistory() {
    const { data } = await api.get('/api/cellar/history')
    return data
  },

  async getWineHistory(wineId) {
    const { data } = await api.get(`/api/cellar/history/wine/${wineId}`)
    return data
  }
}
