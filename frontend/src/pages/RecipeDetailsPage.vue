<template>
  <div>
    <LoadingSpinner v-if="loading" />
    <AlertMessage :message="error" @dismiss="error = ''" />

    <template v-if="recipe">
      <!-- View mode -->
      <div v-if="!editing" class="space-y-8">

        <!-- Header -->
        <div class="flex flex-wrap items-start justify-between gap-4">
          <div>
            <nav class="text-xs text-base-content/40 mb-2 flex items-center gap-1.5">
              <router-link to="/recipes" class="hover:text-primary transition-colors">Meals</router-link>
              <span>›</span>
              <span class="text-base-content/60">{{ recipe.name }}</span>
            </nav>
            <div class="mb-2">
              <span class="badge-pill bg-primary/10 text-primary border border-primary/20">{{ recipe.recipeType }}</span>
            </div>
            <h1 class="font-heading text-3xl font-bold text-base-content leading-tight">{{ recipe.name }}</h1>
          </div>
          <div v-if="authStore.isAdmin" class="flex gap-2 mt-1">
            <button class="btn btn-ghost btn-sm border border-base-200" @click="editing = true">Edit</button>
            <button class="btn btn-ghost btn-sm text-error" @click="confirmDelete">Delete</button>
          </div>
        </div>

        <!-- Decorative rule -->
        <div class="wine-rule"></div>

        <!-- Content grid -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-8">

          <!-- Left: image + description -->
          <div class="space-y-4">
            <div class="rounded-2xl overflow-hidden shadow-sm bg-base-200" style="aspect-ratio:1/1">
              <img
                :src="recipe.imageUrl || 'https://placehold.co/400x400/5D4037/faf8f5?text=🍽️'"
                :alt="recipe.name"
                class="w-full h-full object-cover"
              />
            </div>
            <p v-if="recipe.description" class="text-base-content/55 text-sm leading-relaxed">
              {{ recipe.description }}
            </p>
          </div>

          <!-- Right: ingredients + instructions + pairings -->
          <div class="md:col-span-2 space-y-7">

            <!-- Ingredients -->
            <div>
              <h2 class="section-title mb-4">Ingredients</h2>
              <ul class="space-y-2">
                <li
                  v-for="(ingredient, i) in recipe.ingredients"
                  :key="i"
                  class="flex gap-3 text-sm border-b border-base-200 pb-2 last:border-0"
                >
                  <span class="text-primary font-bold mt-0.5 flex-shrink-0">·</span>
                  <span class="text-base-content/75">{{ ingredient }}</span>
                </li>
              </ul>
            </div>

            <!-- Instructions -->
            <div>
              <h2 class="section-title mb-4">Instructions</h2>
              <div class="text-base-content/65 text-sm leading-relaxed whitespace-pre-line">{{ recipe.instructions }}</div>
            </div>

            <!-- Wine pairings -->
            <div v-if="recipe.pairings?.length || ruleMatchedWines.length">
              <h2 class="section-title mb-4">Wine Pairings</h2>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div
                  v-for="pairing in recipe.pairings"
                  :key="pairing.wineId"
                >
                  <div v-if="pairing.wine" class="flex items-start gap-3 bg-base-100 rounded-xl border border-base-200 p-3 shadow-sm">
                    <span class="text-primary mt-0.5 text-base flex-shrink-0">🍷</span>
                    <div>
                      <router-link :to="`/wines/${pairing.wineId}`" class="font-semibold text-sm hover:text-primary transition-colors">
                        {{ pairing.wine.name }} {{ pairing.wine.year }}
                      </router-link>
                      <p v-if="pairing.notes" class="text-xs text-base-content/45 mt-0.5 italic">{{ pairing.notes }}</p>
                    </div>
                  </div>
                </div>
                <div v-for="wine in ruleMatchedWines" :key="`rule-${wine.wineId}`">
                  <div class="flex items-start gap-3 bg-base-100 rounded-xl border border-base-200 p-3 shadow-sm">
                    <span class="text-base-content/30 mt-0.5 text-base flex-shrink-0">🍷</span>
                    <div>
                      <router-link :to="`/wines/${wine.wineId}`" class="font-semibold text-sm hover:text-primary transition-colors">
                        {{ wine.wineName }} {{ wine.wineYear }}
                      </router-link>
                      <p class="text-xs text-base-content/35 mt-0.5">via rule: {{ wine.ruleName }}</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div v-else>
              <h2 class="section-title mb-3">Wine Pairings</h2>
              <p class="text-base-content/40 text-sm italic">No wine pairings defined for this recipe.</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Edit mode -->
      <div v-else-if="authStore.isAdmin" class="space-y-5">
        <div class="flex items-center gap-3">
          <button class="btn btn-ghost btn-sm" @click="editing = false">← Back</button>
          <h2 class="section-title">Edit Meal</h2>
        </div>
        <RecipeForm :initial-data="recipe" @submit="handleUpdate" @cancel="editing = false" />
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, onMounted }      from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { recipeService }       from '@/services/recipeService'
import { pairingRuleService }  from '@/services/pairingRuleService'
import { useRecipesStore }     from '@/stores/recipes'
import { useAuthStore }        from '@/stores/auth'
import RecipeForm     from '@/components/RecipeForm.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import AlertMessage   from '@/components/AlertMessage.vue'

const route        = useRoute()
const router       = useRouter()
const recipesStore = useRecipesStore()
const authStore    = useAuthStore()

const recipe           = ref(null)
const ruleMatchedWines = ref([])
const loading          = ref(true)
const error            = ref('')
const editing          = ref(false)

async function load() {
  loading.value = true; error.value = ''
  try {
    const id = Number(route.params.id)
    const [recipeData, ruleWines] = await Promise.all([
      recipeService.getById(id),
      pairingRuleService.getWinesForRecipe(id),
    ])
    recipe.value = recipeData
    const directWineIds = new Set(recipeData.pairings?.map(p => p.wineId) ?? [])
    ruleMatchedWines.value = ruleWines.filter(w => !directWineIds.has(w.wineId))
  } catch (err) { error.value = err.message }
  finally { loading.value = false }
}

async function handleUpdate(data) {
  try { recipe.value = await recipesStore.updateRecipe(recipe.value.id, data); editing.value = false }
  catch (err) { error.value = err.message; throw err }
}

async function confirmDelete() {
  if (!confirm(`Delete "${recipe.value.name}"?`)) return
  try { await recipesStore.deleteRecipe(recipe.value.id); router.push('/recipes') }
  catch (err) { error.value = err.message }
}

onMounted(load)
</script>
