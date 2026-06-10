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
    const {
      search, name, domain,
      color, colors,
      country, drinkStatus,
      rank, year,
      appellation,
      cepage, cepages,
      recipeId
    } = filters
    const params = {}
    // Broad backward-compat search
    if (search?.trim())        params.search      = search.trim()
    // Dedicated field filters (new)
    if (name?.trim())          params.name        = name.trim()
    if (domain?.trim())        params.domain      = domain.trim()
    if (appellation?.trim())   params.appellation = appellation.trim()
    // Color: prefer multi-select array; fall back to single string
    if (colors?.length)        params.colors      = colors          // Axios serialises as ?colors=Red&colors=White
    else if (color)            params.color       = color
    // Cépage: prefer multi-select array; fall back to single string
    if (cepages?.length)       params.cepages     = cepages
    else if (cepage?.trim())   params.cepage      = cepage.trim()
    // Other facets
    if (country)               params.country     = country
    if (drinkStatus)           params.drinkStatus = drinkStatus
    if (rank)                  params.rank        = rank
    if (year)                  params.year        = year
    if (recipeId)              params.recipeId    = recipeId
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
  },

  async uploadImage(file) {
    const form = new FormData()
    form.append('file', file)
    const { data } = await api.post('/api/wines/upload-image', form, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    return data.imageUrl
  }
}
