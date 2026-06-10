import api from './api'

function normalizePairing(p) {
  return {
    wineId: p.wineId,
    notes: p.notes,
    wine: { name: p.wineName, year: p.wineYear }
  }
}

function normalizeRecipe(recipe) {
  return { ...recipe, pairings: (recipe.pairings || []).map(normalizePairing) }
}

export const recipeService = {
  async getAll(search = '', recipeType = '') {
    const params = {}
    if (search.trim())  params.search     = search.trim()
    if (recipeType)     params.recipeType = recipeType
    const { data } = await api.get('/api/recipes', { params })
    return data.map(normalizeRecipe)
  },

  async getById(id) {
    const { data } = await api.get(`/api/recipes/${id}`)
    return normalizeRecipe(data)
  },

  async create(data) {
    const { data: recipe } = await api.post('/api/recipes', data)
    return normalizeRecipe(recipe)
  },

  async update(id, data) {
    const { data: recipe } = await api.put(`/api/recipes/${id}`, data)
    return normalizeRecipe(recipe)
  },

  async delete(id) {
    await api.delete(`/api/recipes/${id}`)
  }
}
