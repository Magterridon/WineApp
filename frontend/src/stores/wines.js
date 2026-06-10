import { defineStore } from 'pinia'
import { ref } from 'vue'
import { wineService } from '../services/wineService'

export const useWinesStore = defineStore('wines', () => {
  const wines = ref([])
  const loading = ref(false)
  const error = ref(null)

  async function fetchWines(filters = {}) {
    loading.value = true
    error.value = null
    try {
      wines.value = await wineService.getAll(filters)
    } catch (err) {
      error.value = err.message
    } finally {
      loading.value = false
    }
  }

  async function createWine(data) {
    const wine = await wineService.create(data)
    wines.value.unshift(wine)
    return wine
  }

  async function updateWine(id, data) {
    const updated = await wineService.update(id, data)
    const idx = wines.value.findIndex(w => w.id === id)
    if (idx !== -1) wines.value[idx] = updated
    return updated
  }

  async function deleteWine(id) {
    await wineService.delete(id)
    wines.value = wines.value.filter(w => w.id !== id)
  }

  return { wines, loading, error, fetchWines, createWine, updateWine, deleteWine }
})
