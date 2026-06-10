<template>
  <div>
    <LoadingSpinner v-if="loading" />

    <AlertMessage :message="error" @dismiss="error = ''" />

    <template v-if="recipe">
      <div v-if="!editing">
        <div class="d-flex justify-content-between align-items-start mb-3">
          <div>
            <nav aria-label="breadcrumb">
              <ol class="breadcrumb mb-1">
                <li class="breadcrumb-item"><router-link to="/recipes">Recipes</router-link></li>
                <li class="breadcrumb-item active">{{ recipe.name }}</li>
              </ol>
            </nav>
            <h2 class="fw-bold mb-0">{{ recipe.name }}</h2>
            <span class="badge text-white mt-1" style="background-color: #4a1020;">{{ recipe.recipeType }}</span>
          </div>
          <div v-if="authStore.isAdmin" class="d-flex gap-2 mt-2">
            <button class="btn btn-outline-secondary btn-sm" @click="editing = true">Edit</button>
            <button class="btn btn-outline-danger btn-sm" @click="confirmDelete">Delete</button>
          </div>
        </div>

        <div class="row g-4">
          <div class="col-md-4">
            <img
              :src="recipe.imageUrl || 'https://placehold.co/400x300/5D4037/white?text=Recipe'"
              class="img-fluid rounded shadow-sm mb-3"
              :alt="recipe.name"
            />
            <p v-if="recipe.description" class="text-muted">{{ recipe.description }}</p>
          </div>

          <div class="col-md-8">
            <div class="mb-4">
              <h5 class="fw-bold">Ingredients</h5>
              <ul class="list-group list-group-flush">
                <li v-for="(ingredient, i) in recipe.ingredients" :key="i" class="list-group-item ps-0">
                  {{ ingredient }}
                </li>
              </ul>
            </div>

            <div class="mb-4">
              <h5 class="fw-bold">Instructions</h5>
              <div class="text-muted" style="white-space: pre-line;">{{ recipe.instructions }}</div>
            </div>

            <div v-if="recipe.pairings?.length">
              <h5 class="fw-bold">Wine Pairings</h5>
              <div class="row g-2">
                <div v-for="pairing in recipe.pairings" :key="pairing.wineId" class="col-md-6">
                  <div v-if="pairing.wine" class="card border-0 shadow-sm h-100">
                    <div class="card-body py-2">
                      <router-link
                        :to="`/wines/${pairing.wineId}`"
                        class="text-decoration-none text-dark fw-semibold"
                      >🍷 {{ pairing.wine.name }} {{ pairing.wine.year }}</router-link>
                      <p v-if="pairing.notes" class="small text-muted mb-0 mt-1 fst-italic">{{ pairing.notes }}</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <p v-else class="text-muted">No wine pairings defined.</p>
          </div>
        </div>
      </div>

      <div v-else-if="authStore.isAdmin">
        <div class="d-flex align-items-center mb-3">
          <button class="btn btn-link p-0 me-2" @click="editing = false">← Back</button>
          <h4 class="fw-bold mb-0">Edit Recipe</h4>
        </div>
        <RecipeForm
          :initial-data="recipe"
          @submit="handleUpdate"
          @cancel="editing = false"
        />
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { recipeService } from '@/services/recipeService'
import { useRecipesStore } from '@/stores/recipes'
import { useAuthStore } from '@/stores/auth'
import RecipeForm from '@/components/RecipeForm.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import AlertMessage from '@/components/AlertMessage.vue'

const route = useRoute()
const router = useRouter()
const recipesStore = useRecipesStore()
const authStore = useAuthStore()

const recipe = ref(null)
const loading = ref(true)
const error = ref('')
const editing = ref(false)

async function load() {
  loading.value = true
  error.value = ''
  try {
    recipe.value = await recipeService.getById(Number(route.params.id))
  } catch (err) {
    error.value = err.message
  } finally {
    loading.value = false
  }
}

async function handleUpdate(data) {
  try {
    recipe.value = await recipesStore.updateRecipe(recipe.value.id, data)
    editing.value = false
  } catch (err) {
    error.value = err.message
    throw err
  }
}

async function confirmDelete() {
  if (!confirm(`Delete "${recipe.value.name}"?`)) return
  try {
    await recipesStore.deleteRecipe(recipe.value.id)
    router.push('/recipes')
  } catch (err) {
    error.value = err.message
  }
}

onMounted(load)
</script>
