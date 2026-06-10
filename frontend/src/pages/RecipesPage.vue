<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2 class="fw-bold mb-0">Recipes</h2>
      <button
        v-if="authStore.isAdmin"
        class="btn text-white"
        style="background-color: #4a1020;"
        @click="showForm = !showForm"
        data-testid="create-recipe-btn"
      >
        {{ showForm ? '✕ Cancel' : '+ Add Recipe' }}
      </button>
    </div>

    <div v-if="showForm" class="card mb-4 shadow-sm">
      <div class="card-body">
        <h5 class="card-title mb-3">Add New Recipe</h5>
        <RecipeForm
          @submit="handleCreate"
          @cancel="showForm = false"
        />
      </div>
    </div>

    <AlertMessage :message="error" @dismiss="error = ''" />
    <AlertMessage :message="successMsg" type="success" :dismissible="false" />

    <div class="mb-3">
      <SearchBar v-model="search" placeholder="Search by name, ingredient, type, wine pairing..." />
    </div>

    <LoadingSpinner v-if="recipesStore.loading" />

    <template v-else>
      <p class="text-muted small mb-3">{{ recipesStore.recipes.length }} recipe{{ recipesStore.recipes.length !== 1 ? 's' : '' }} found</p>

      <div v-if="recipesStore.recipes.length === 0" class="text-center py-5 text-muted">
        <p>No recipes found. Try a different search or add a new recipe.</p>
      </div>

      <div v-else class="row row-cols-1 row-cols-sm-2 row-cols-md-3 row-cols-xl-4 g-3">
        <div v-for="recipe in recipesStore.recipes" :key="recipe.id" class="col">
          <RecipeCard :recipe="recipe" />
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import { useRecipesStore } from '@/stores/recipes'
import { useAuthStore } from '@/stores/auth'
import RecipeCard from '@/components/RecipeCard.vue'
import RecipeForm from '@/components/RecipeForm.vue'
import SearchBar from '@/components/SearchBar.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import AlertMessage from '@/components/AlertMessage.vue'

const recipesStore = useRecipesStore()
const authStore = useAuthStore()

const search = ref('')
const showForm = ref(false)
const error = ref('')
const successMsg = ref('')

let searchTimer

watch(search, val => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => recipesStore.fetchRecipes(val), 300)
})

async function handleCreate(data) {
  error.value = ''
  try {
    await recipesStore.createRecipe(data)
    showForm.value = false
    successMsg.value = 'Recipe added successfully!'
    setTimeout(() => { successMsg.value = '' }, 3000)
  } catch (err) {
    error.value = err.message
    throw err
  }
}

onMounted(() => recipesStore.fetchRecipes())
</script>
