import api from './api'

export function normalizeWine(wine) {
  return {
    ...wine,
    cepages: (wine.cepages || []).map(c => ({ name: c.cepageName, percentage: c.percentage }))
  }
}

function toPayload(data) {
  return {
    ...data,
    cepages: (data.cepages || [])
      .filter(c => c.name?.trim())
      .map(c => ({ cepageName: c.name, percentage: c.percentage }))
  }
}

export const wineService = {
  async getAll(filters = {}) {
    const { search, color, country, drinkStatus } = filters
    const params = {}
    if (search?.trim()) params.search = search.trim()
    if (color) params.color = color
    if (country) params.country = country
    if (drinkStatus) params.drinkStatus = drinkStatus
    const { data } = await api.get('/api/wines', { params })
    return data.map(normalizeWine)
  },

  async getSimilar(name) {
    if (!name?.trim()) return []
    const { data } = await api.get('/api/wines/similar', { params: { name: name.trim() } })
    return data.map(normalizeWine)
  },

  async getById(id) {
    const { data } = await api.get(`/api/wines/${id}`)
    return normalizeWine(data)
  },

  async create(data) {
    const { data: wine } = await api.post('/api/wines', toPayload(data))
    return normalizeWine(wine)
  },

  async update(id, data) {
    const { data: wine } = await api.put(`/api/wines/${id}`, toPayload(data))
    return normalizeWine(wine)
  },

  async delete(id) {
    await api.delete(`/api/wines/${id}`)
  }
}
