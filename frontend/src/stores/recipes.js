import { defineStore } from 'pinia'
import { ref } from 'vue'
import { recipeService } from '../services/recipeService'

export const useRecipesStore = defineStore('recipes', () => {
  const recipes = ref([])
  const loading = ref(false)
  const error = ref(null)

  async function fetchRecipes(search = '', recipeType = '') {
    loading.value = true
    error.value = null
    try {
      recipes.value = await recipeService.getAll(search, recipeType)
    } catch (err) {
      error.value = err.message
    } finally {
      loading.value = false
    }
  }

  async function createRecipe(data) {
    const recipe = await recipeService.create(data)
    recipes.value.push(recipe)
    return recipe
  }

  async function updateRecipe(id, data) {
    const updated = await recipeService.update(id, data)
    const idx = recipes.value.findIndex(r => r.id === id)
    if (idx !== -1) recipes.value[idx] = updated
    return updated
  }

  async function deleteRecipe(id) {
    await recipeService.delete(id)
    recipes.value = recipes.value.filter(r => r.id !== id)
  }

  return { recipes, loading, error, fetchRecipes, createRecipe, updateRecipe, deleteRecipe }
})
