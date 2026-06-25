import api from './api'

export const pairingRuleService = {
  // ── Admin CRUD ──────────────────────────────────────────────────────────────

  async getAll() {
    const { data } = await api.get('/api/admin/pairing-rules')
    return data
  },

  async getById(id) {
    const { data } = await api.get(`/api/admin/pairing-rules/${id}`)
    return data
  },

  async create(rule) {
    const { data } = await api.post('/api/admin/pairing-rules', rule)
    return data
  },

  async update(id, rule) {
    const { data } = await api.put(`/api/admin/pairing-rules/${id}`, rule)
    return data
  },

  async toggle(id) {
    const { data } = await api.patch(`/api/admin/pairing-rules/${id}/toggle`)
    return data
  },

  async remove(id) {
    await api.delete(`/api/admin/pairing-rules/${id}`)
  },

  // ── Evaluation (used by Weekly Menu) ────────────────────────────────────────

  async getCandidates(wineIds) {
    if (!wineIds?.length) return []
    const { data } = await api.get('/api/pairing-rules/candidates', {
      params: { wineIds }
    })
    return data
  },

  async getWinesForRecipe(recipeId) {
    const { data } = await api.get(`/api/pairing-rules/wines-for-recipe/${recipeId}`)
    return data
  }
}
