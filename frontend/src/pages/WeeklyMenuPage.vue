<template>
  <div class="space-y-8">

    <!-- Header -->
    <div class="space-y-2">
      <p class="label-caps text-primary">Weekly Menu</p>
      <h1 class="font-heading text-3xl font-bold text-base-content leading-tight">Your Table for the Week</h1>
      <div class="flex items-center gap-3">
        <p class="text-base-content/50 text-sm">{{ weekLabel }} · Curated from your cellar</p>
        <span class="text-xs font-semibold text-primary/60 bg-primary/8 border border-primary/14 rounded-full px-3 py-1">
          Week {{ weekNumber }}
        </span>
      </div>
      <div class="h-px mt-3" style="background: linear-gradient(to right, #4a1020 0%, rgba(74,16,32,.15) 55%, transparent 100%);"></div>
    </div>

    <LoadingSpinner v-if="loading" />
    <AlertMessage :message="error" @dismiss="error = ''" />

    <template v-if="!loading && !error">

      <!-- Empty: no cellar wines -->
      <div v-if="cellarItems.length === 0" class="text-center py-16 text-base-content/40">
        <div class="text-5xl mb-3">🍾</div>
        <h4 class="font-heading font-bold text-base-content/70 text-lg mt-3">Your cellar is empty</h4>
        <p class="text-sm mt-1">Add wines to your cellar to generate a weekly menu.</p>
        <router-link to="/wines" class="btn btn-primary btn-sm mt-4">Browse Wines</router-link>
      </div>

      <!-- Empty: no recipes -->
      <div v-else-if="recipes.length === 0" class="text-center py-16 text-base-content/40">
        <div class="text-5xl mb-3">📋</div>
        <h4 class="font-heading font-bold text-base-content/70 text-lg mt-3">No meals found</h4>
        <p class="text-sm mt-1">Add some meals to generate a weekly pairing menu.</p>
        <router-link to="/recipes" class="btn btn-primary btn-sm mt-4">Add Meals</router-link>
      </div>

      <!-- Menu -->
      <template v-else>
        <div v-for="(course, i) in menu" :key="course.type" class="space-y-3">
          <div class="flex items-center gap-3">
            <span class="text-xs font-bold text-primary/30 tracking-widest">{{ i + 1 }}</span>
            <div class="flex-1 h-px bg-primary/10"></div>
          </div>
          <MenuCourseRow :label="course.label" :wine="course.wine" :recipe="course.recipe" />
        </div>

        <p v-if="incompleteCount > 0" class="text-center text-base-content/40 text-sm italic">
          {{ partialNote }}
        </p>

        <p class="text-center text-base-content/30 text-xs pb-2">
          🗓 Menu refreshes every Sunday · Based on what's ready to drink in your cellar
        </p>
      </template>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useCellarStore }     from '@/stores/cellar'
import { recipeService }      from '@/services/recipeService'
import { pairingRuleService } from '@/services/pairingRuleService'
import { composeMenu, getWeekSeed, getWeekLabel } from '@/utils/menuComposer'
import MenuCourseRow  from '@/components/MenuCourseRow.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import AlertMessage   from '@/components/AlertMessage.vue'

const cellarStore    = useCellarStore()
const recipes        = ref([])
const ruleCandidates = ref([])
const loading        = ref(true)
const error          = ref('')

const seed      = getWeekSeed()
const weekLabel = getWeekLabel()
const weekNumber = seed % 100

const cellarItems = computed(() => cellarStore.items)

const menu = computed(() => {
  if (cellarItems.value.length === 0 || recipes.value.length === 0) return []
  return composeMenu(cellarItems.value, recipes.value, seed, ruleCandidates.value)
})

const incompleteCount = computed(() => menu.value.filter(c => !c.wine || !c.recipe).length)

const partialNote = computed(() => {
  const n = incompleteCount.value
  if (n === 3) return 'Add wines to your cellar and meals to generate a full menu.'
  return `${n} course${n > 1 ? 's' : ''} couldn't be fully paired — add more meals or cellar wines to complete the menu.`
})

onMounted(async () => {
  loading.value = true; error.value = ''
  try {
    await cellarStore.fetchCellar()
    recipes.value = await recipeService.getAll()
    const wineIds = cellarStore.items.map(i => i.wineId).filter(Boolean)
    if (wineIds.length > 0) ruleCandidates.value = await pairingRuleService.getCandidates(wineIds)
  } catch (err) { error.value = err.message }
  finally { loading.value = false }
})
</script>
