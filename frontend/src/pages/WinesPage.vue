<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2 class="fw-bold mb-0">Wines</h2>
      <button
        class="btn text-white"
        style="background-color: #4a1020;"
        @click="showForm = !showForm"
        data-testid="create-wine-btn"
      >
        {{ showForm ? '✕ Cancel' : '+ Add Wine' }}
      </button>
    </div>

    <div v-if="showForm" class="card mb-4 shadow-sm">
      <div class="card-body">
        <h5 class="card-title mb-3">Add New Wine</h5>
        <WineForm @submit="handleCreate" @cancel="showForm = false" />
      </div>
    </div>

    <AlertMessage :message="error" @dismiss="error = ''" />
    <AlertMessage :message="successMsg" type="success" :dismissible="false" />

    <DrinkModal
      :show="!!drinkTarget"
      :wine-id="drinkTarget?.id"
      :wine-name="drinkTarget?.name"
      @confirm="confirmDrink"
      @cancel="drinkTarget = null"
    />

    <!-- Search + filters -->
    <div class="mb-3">
      <SearchBar v-model="search" placeholder="Search by name, domain, region, cépage..." />
    </div>

    <div class="d-flex flex-wrap gap-2 mb-3 align-items-center">
      <!-- Color filter pills -->
      <button
        class="btn btn-sm"
        :class="activeColor === '' ? 'btn-dark' : 'btn-outline-secondary'"
        @click="setColor('')"
      >All colors</button>
      <button
        v-for="c in WINE_COLORS"
        :key="c"
        class="btn btn-sm"
        :class="activeColor === c ? 'text-white' : 'btn-outline-secondary'"
        :style="activeColor === c ? `background-color: ${COLOR_STYLES[c]?.bg}; border-color: ${COLOR_STYLES[c]?.bg};` : ''"
        @click="setColor(c)"
      >{{ c }}</button>

      <!-- Drink status filter -->
      <select v-model="activeDrinkStatus" class="form-select form-select-sm ms-auto" style="width: auto;" @change="applyFilters">
        <option v-for="opt in DRINK_STATUS_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
      </select>
    </div>

    <LoadingSpinner v-if="winesStore.loading" />

    <template v-else>
      <p class="text-muted small mb-3">{{ winesStore.wines.length }} wine{{ winesStore.wines.length !== 1 ? 's' : '' }} found</p>

      <div v-if="winesStore.wines.length === 0" class="text-center py-5 text-muted">
        <div style="font-size: 3rem;">🍷</div>
        <p class="mt-2">No wines found. Try a different search or add a new wine.</p>
      </div>

      <div v-else class="row row-cols-1 row-cols-sm-2 row-cols-md-3 row-cols-xl-4 g-3">
        <div v-for="wine in winesStore.wines" :key="wine.id" class="col">
          <WineCard
            :wine="wine"
            :in-cellar="cellarStore.isInCellar(wine.id)"
            @add-to-cellar="addToCellar"
            @remove-from-cellar="removeFromCellar"
            @drink="openDrinkModal"
          />
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import { useWinesStore } from '@/stores/wines'
import { useCellarStore } from '@/stores/cellar'
import WineCard from '@/components/WineCard.vue'
import WineForm from '@/components/WineForm.vue'
import DrinkModal from '@/components/DrinkModal.vue'
import SearchBar from '@/components/SearchBar.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import AlertMessage from '@/components/AlertMessage.vue'
import { WINE_COLORS, COLOR_STYLES, DRINK_STATUS_OPTIONS } from '@/utils/drinkStatus'

const winesStore = useWinesStore()
const cellarStore = useCellarStore()

const search = ref('')
const activeColor = ref('')
const activeDrinkStatus = ref('')
const showForm = ref(false)
const error = ref('')
const successMsg = ref('')
const drinkTarget = ref(null)

let searchTimer

function buildFilters() {
  return {
    search: search.value,
    color: activeColor.value,
    drinkStatus: activeDrinkStatus.value
  }
}

function applyFilters() {
  winesStore.fetchWines(buildFilters())
}

watch(search, () => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(applyFilters, 300)
})

function setColor(c) {
  activeColor.value = c
  applyFilters()
}

async function handleCreate(data) {
  error.value = ''
  try {
    await winesStore.createWine(data)
    showForm.value = false
    successMsg.value = 'Wine added successfully!'
    setTimeout(() => { successMsg.value = '' }, 3000)
  } catch (err) {
    error.value = err.message
    throw err
  }
}

async function addToCellar(wineId) {
  try {
    await cellarStore.addWine(wineId)
  } catch (err) {
    error.value = err.message
  }
}

async function removeFromCellar(wineId) {
  await cellarStore.removeWine(wineId)
}

function openDrinkModal(wineId) {
  const wine = winesStore.wines.find(w => w.id === wineId)
  drinkTarget.value = wine ? { id: wine.id, name: `${wine.name} ${wine.year}` } : { id: wineId, name: '' }
}

async function confirmDrink(form) {
  await cellarStore.drinkBottle(drinkTarget.value.id, form)
  drinkTarget.value = null
  successMsg.value = 'Bottle recorded!'
  setTimeout(() => { successMsg.value = '' }, 3000)
}

onMounted(() => {
  winesStore.fetchWines()
  cellarStore.fetchCellar()
})
</script>
