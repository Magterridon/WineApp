import api from './api'
import { normalizeWine } from './wineService'

function normalizeItem(item) {
  return { ...item, wine: item.wine ? normalizeWine(item.wine) : null }
}

export const cellarService = {
  async getCellar() {
    const { data } = await api.get('/api/cellar')
    return (data.items || []).map(normalizeItem)
  },

  async addWine(wineId) {
    const { data } = await api.post('/api/cellar/items', { wineId })
    return normalizeItem(data)
  },

  async increment(wineId) {
    const { data } = await api.patch(`/api/cellar/items/${wineId}/increment`)
    return normalizeItem(data)
  },

  async decrement(wineId) {
    const response = await api.patch(`/api/cellar/items/${wineId}/decrement`)
    if (response.status === 204 || !response.data) return null
    return normalizeItem(response.data)
  },

  async removeWine(wineId) {
    await api.delete(`/api/cellar/items/${wineId}`)
  }
}
