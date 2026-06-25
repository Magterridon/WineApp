<template>
  <div class="space-y-6">

    <PageHeader eyebrow="Meals" title="Meals">
      <button
        v-if="authStore.isAdmin"
        class="btn btn-primary btn-sm"
        @click="showForm = !showForm"
        data-testid="create-recipe-btn"
      >{{ showForm ? '✕ Cancel' : '+ Add Meal' }}</button>
    </PageHeader>

    <!-- Create form -->
    <div v-if="showForm" class="bg-base-100 rounded-2xl border border-base-200 p-6 shadow-sm">
      <h2 class="section-title mb-5">Add New Meal</h2>
      <RecipeForm @submit="handleCreate" @cancel="showForm = false" />
    </div>

    <AlertMessage :message="error" @dismiss="error = ''" />
    <AlertMessage :message="successMsg" type="success" :dismissible="false" />

    <!-- Filters -->
    <div class="filter-panel">
      <input
        v-model="search"
        type="text"
        class="input input-bordered input-sm w-full"
        placeholder="Search by name, ingredient, wine pairing…"
      />
      <div class="flex flex-wrap gap-2">
        <button
          class="btn btn-sm"
          :class="activeType === '' ? 'btn-primary' : 'btn-ghost border border-base-300'"
          @click="setType('')"
        >All</button>
        <button
          v-for="t in RECIPE_TYPES"
          :key="t.value"
          class="btn btn-sm"
          :class="activeType === t.value ? 'btn-primary' : 'btn-ghost border border-base-300'"
          @click="setType(t.value)"
        >{{ t.label }}</button>
      </div>
    </div>

    <LoadingSpinner v-if="recipesStore.loading" />

    <template v-else>
      <p class="text-xs text-base-content/40 font-medium tracking-wide">
        {{ recipesStore.recipes.length }} meal{{ recipesStore.recipes.length !== 1 ? 's' : '' }} found
      </p>

      <EmptyState
        v-if="recipesStore.recipes.length === 0"
        icon="🍽️"
        title="No meals found"
        body="Try adjusting your search, or add a new meal."
      />

      <!-- Grid — square cards work well at 3–4 columns -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
        <RecipeCard v-for="recipe in recipesStore.recipes" :key="recipe.id" :recipe="recipe" />
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import { useRecipesStore } from '@/stores/recipes'
import { useAuthStore }    from '@/stores/auth'
import RecipeCard     from '@/components/RecipeCard.vue'
import RecipeForm     from '@/components/RecipeForm.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import AlertMessage   from '@/components/AlertMessage.vue'
import PageHeader     from '@/components/ui/PageHeader.vue'
import EmptyState     from '@/components/ui/EmptyState.vue'

const RECIPE_TYPES = [
  { value: 'Starter', label: 'Starter' },
  { value: 'Main',    label: 'Main' },
  { value: 'Dessert', label: 'Dessert' },
  { value: 'Other',   label: 'Other' },
]

const recipesStore = useRecipesStore()
const authStore    = useAuthStore()

const search     = ref('')
const activeType = ref('')
const showForm   = ref(false)
const error      = ref('')
const successMsg = ref('')
let searchTimer

function applyFilters() { recipesStore.fetchRecipes(search.value, activeType.value) }
function setType(t) { activeType.value = t; applyFilters() }

watch(search, () => { clearTimeout(searchTimer); searchTimer = setTimeout(applyFilters, 300) })

async function handleCreate(data) {
  error.value = ''
  try {
    await recipesStore.createRecipe(data)
    showForm.value = false; successMsg.value = 'Meal added!'
    setTimeout(() => { successMsg.value = '' }, 3000)
  } catch (err) { error.value = err.message; throw err }
}

onMounted(() => applyFilters())
</script>
