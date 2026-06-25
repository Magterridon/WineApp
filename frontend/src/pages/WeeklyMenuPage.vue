<template>
  <div class="space-y-8">

    <PageHeader eyebrow="Weekly Menu" title="Your Table for the Week">
      <span class="badge-pill bg-primary/10 text-primary border border-primary/20 text-xs font-semibold">
        Week {{ weekNumber }}
      </span>
    </PageHeader>
    <p class="text-base-content/50 text-sm -mt-4">{{ weekLabel }} · Curated from your cellar</p>

    <LoadingSpinner v-if="loading" />
    <AlertMessage :message="error" @dismiss="error = ''" />

    <template v-if="!loading && !error">

      <!-- Empty: no cellar wines -->
      <EmptyState
        v-if="cellarItems.length === 0"
        icon="🍾"
        title="Your cellar is empty"
        body="Add wines to your cellar to generate a weekly menu."
      >
        <router-link to="/wines" class="btn btn-primary btn-sm">Browse Wines</router-link>
      </EmptyState>

      <!-- Empty: no recipes -->
      <EmptyState
        v-else-if="recipes.length === 0"
        icon="📋"
        title="No meals found"
        body="Add some meals to generate a weekly pairing menu."
      >
        <router-link to="/recipes" class="btn btn-primary btn-sm">Add Meals</router-link>
      </EmptyState>

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
import PageHeader     from '@/components/ui/PageHeader.vue'
import EmptyState     from '@/components/ui/EmptyState.vue'

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
