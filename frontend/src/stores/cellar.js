import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { cellarService } from '../services/cellarService'
import { drinkService } from '../services/drinkService'

export const useCellarStore = defineStore('cellar', () => {
  const items = ref([])
  const history = ref([])
  const loading = ref(false)
  const error = ref(null)

  const wineIds = computed(() => new Set(items.value.map(i => i.wineId)))

  function isInCellar(wineId) {
    return wineIds.value.has(wineId)
  }

  function getBottleCount(wineId) {
    return items.value.find(i => i.wineId === wineId)?.bottleCount ?? 0
  }

  async function fetchCellar() {
    loading.value = true
    error.value = null
    try {
      items.value = await cellarService.getCellar()
    } catch (err) {
      error.value = err.message
    } finally {
      loading.value = false
    }
  }

  async function fetchHistory() {
    try {
      history.value = await drinkService.getHistory()
    } catch (err) {
      error.value = err.message
    }
  }

  async function addWine(wineId) {
    try {
      const item = await cellarService.addWine(wineId)
      const idx = items.value.findIndex(i => i.wineId === wineId)
      if (idx !== -1) {
        items.value[idx] = item
      } else {
        items.value.push(item)
      }
    } catch (err) {
      error.value = err.message
      throw err
    }
  }

  async function increment(wineId) {
    try {
      const item = await cellarService.increment(wineId)
      const idx = items.value.findIndex(i => i.wineId === wineId)
      if (idx !== -1) items.value[idx] = item
    } catch (err) {
      error.value = err.message
    }
  }

  async function decrement(wineId) {
    try {
      const item = await cellarService.decrement(wineId)
      if (item === null) {
        items.value = items.value.filter(i => i.wineId !== wineId)
      } else {
        const idx = items.value.findIndex(i => i.wineId === wineId)
        if (idx !== -1) items.value[idx] = item
      }
    } catch (err) {
      error.value = err.message
    }
  }

  async function removeWine(wineId) {
    try {
      await cellarService.removeWine(wineId)
      items.value = items.value.filter(i => i.wineId !== wineId)
    } catch (err) {
      error.value = err.message
    }
  }

  async function drinkBottle(wineId, form) {
    try {
      const result = await drinkService.drinkBottle(wineId, form)
      // Update cellar: remove or update the item
      if (result.cellarItem === null) {
        items.value = items.value.filter(i => i.wineId !== wineId)
      } else {
        const idx = items.value.findIndex(i => i.wineId === wineId)
        if (idx !== -1) items.value[idx] = result.cellarItem
      }
      // Prepend the new record to history
      history.value.unshift(result.drinkRecord)
    } catch (err) {
      error.value = err.message
      throw err
    }
  }

  return {
    items, history, loading, error,
    wineIds, isInCellar, getBottleCount,
    fetchCellar, fetchHistory,
    addWine, increment, decrement, removeWine, drinkBottle
  }
})
