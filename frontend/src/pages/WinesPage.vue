<template>
  <div class="space-y-6">

    <PageHeader eyebrow="Catalog" title="Wines">
      <button class="btn btn-primary btn-sm" @click="showForm = !showForm" data-testid="create-wine-btn">
        {{ showForm ? '✕ Cancel' : '+ Add Wine' }}
      </button>
    </PageHeader>

    <!-- Create form -->
    <div v-if="showForm" class="bg-base-100 rounded-2xl border border-base-200 p-6 shadow-sm">
      <h2 class="section-title mb-5">Add New Wine</h2>
      <WineForm @submit="handleCreate" @cancel="showForm = false" />
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

    <!-- Filters -->
    <div class="filter-panel">

      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <input v-model="f.name" type="text" class="input input-bordered input-sm w-full" placeholder="Name…" />
        <input v-model="f.domain" type="text" class="input input-bordered input-sm w-full" placeholder="Domain…" />
        <input v-model="f.appellation" type="text" class="input input-bordered input-sm w-full" placeholder="Appellation…" />
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div>
          <label class="label-caps mb-1.5 block">Cépage</label>
          <CepageSelect v-model="f.cepages" />
        </div>

        <div>
          <label class="label-caps mb-1.5 block">Pairing</label>
          <div class="relative">
            <input
              v-model="pairingSearch"
              type="text"
              class="input input-bordered input-sm w-full pr-8"
              :placeholder="f.recipeId ? selectedRecipeName : 'Search recipe…'"
              @input="onPairingInput"
              @focus="showPairingDropdown = true"
              @blur="hidePairingDropdown"
            />
            <button
              v-if="f.recipeId"
              type="button"
              class="absolute right-2 top-1/2 -translate-y-1/2 text-base-content/40 hover:text-base-content"
              @click.prevent="clearPairing"
            >×</button>
            <ul
              v-if="showPairingDropdown && pairingResults.length"
              class="absolute z-30 w-full mt-1 bg-base-100 border border-base-300 rounded-xl shadow-lg max-h-44 overflow-y-auto"
            >
              <li
                v-for="r in pairingResults"
                :key="r.id"
                class="px-3 py-2 text-sm hover:bg-base-200 cursor-pointer"
                @mousedown.prevent="selectPairing(r)"
              >
                {{ r.name }}
                <span class="text-base-content/40 ml-1 text-xs">{{ r.recipeType }}</span>
              </li>
            </ul>
          </div>
        </div>
      </div>

      <div>
        <label class="label-caps mb-1.5 block">Color</label>
        <WineColorPicker v-model="f.colors" />
      </div>

      <div class="flex flex-wrap gap-2 items-center">
        <input
          v-model="f.year"
          type="number"
          class="input input-bordered input-sm w-24"
          placeholder="Vintage"
          min="1900" max="2100"
          @change="applyFilters"
        />
        <select v-model="f.rank" class="select select-bordered select-sm" @change="applyFilters">
          <option value="">All ranks</option>
          <option value="5">★★★★★</option>
          <option value="4">★★★★</option>
          <option value="3">★★★</option>
          <option value="2">★★</option>
          <option value="1">★</option>
        </select>
        <select v-model="f.drinkStatus" class="select select-bordered select-sm" @change="applyFilters">
          <option v-for="opt in DRINK_STATUS_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
        </select>
        <button v-if="hasActiveFilters" class="btn btn-xs btn-ghost ml-auto" @click="clearFilters">✕ Clear filters</button>
      </div>
    </div>

    <LoadingSpinner v-if="winesStore.loading" />

    <template v-else>
      <!-- Result count -->
      <p class="text-xs text-base-content/40 font-medium tracking-wide">
        {{ winesStore.wines.length }} wine{{ winesStore.wines.length !== 1 ? 's' : '' }} found
      </p>

      <EmptyState
        v-if="winesStore.wines.length === 0"
        icon="🍷"
        title="No wines found"
        body="Try adjusting your filters, or add a new wine to the catalog."
      />

      <!-- Grid — 3 cols max for portrait cards -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
        <WineCard
          v-for="wine in winesStore.wines"
          :key="wine.id"
          :wine="wine"
          :in-cellar="cellarStore.isInCellar(wine.id)"
          @add-to-cellar="addToCellar"
          @remove-from-cellar="removeFromCellar"
          @drink="openDrinkModal"
        />
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { useWinesStore }  from '@/stores/wines'
import { useCellarStore } from '@/stores/cellar'
import { recipeService }  from '@/services/recipeService'
import WineCard        from '@/components/WineCard.vue'
import WineForm        from '@/components/WineForm.vue'
import DrinkModal      from '@/components/DrinkModal.vue'
import WineColorPicker from '@/components/WineColorPicker.vue'
import CepageSelect    from '@/components/CepageSelect.vue'
import LoadingSpinner  from '@/components/LoadingSpinner.vue'
import AlertMessage    from '@/components/AlertMessage.vue'
import PageHeader      from '@/components/ui/PageHeader.vue'
import EmptyState      from '@/components/ui/EmptyState.vue'
import { DRINK_STATUS_OPTIONS } from '@/utils/drinkStatus'

const winesStore  = useWinesStore()
const cellarStore = useCellarStore()

const f = reactive({
  name: '', domain: '', appellation: '',
  cepages: [], colors: [], year: '', rank: '', drinkStatus: '', recipeId: null,
})

const showForm    = ref(false)
const error       = ref('')
const successMsg  = ref('')
const drinkTarget = ref(null)

const pairingSearch      = ref('')
const selectedRecipeName = ref('')
const pairingResults     = ref([])
const showPairingDropdown = ref(false)
let pairingTimer = null

function onPairingInput() {
  clearTimeout(pairingTimer)
  f.recipeId = null; selectedRecipeName.value = ''
  pairingTimer = setTimeout(async () => {
    if (!pairingSearch.value.trim()) { pairingResults.value = []; return }
    try { pairingResults.value = (await recipeService.getAll(pairingSearch.value)).slice(0, 8) }
    catch { pairingResults.value = [] }
  }, 250)
}

function selectPairing(recipe) {
  f.recipeId = recipe.id; selectedRecipeName.value = recipe.name
  pairingSearch.value = recipe.name; pairingResults.value = []
  showPairingDropdown.value = false; applyFilters()
}

function clearPairing() {
  f.recipeId = null; selectedRecipeName.value = ''; pairingSearch.value = ''; pairingResults.value = []
  applyFilters()
}

function hidePairingDropdown() {
  setTimeout(() => { showPairingDropdown.value = false }, 150)
}

const hasActiveFilters = computed(() =>
  !!f.name || !!f.domain || !!f.appellation || f.cepages.length > 0 ||
  f.colors.length > 0 || !!f.year || !!f.rank || !!f.drinkStatus || !!f.recipeId
)

function buildFilters() {
  return {
    name:        f.name        || undefined,
    domain:      f.domain      || undefined,
    appellation: f.appellation || undefined,
    cepages:     f.cepages.length ? f.cepages : undefined,
    colors:      f.colors.length  ? f.colors  : undefined,
    year:        f.year        || undefined,
    rank:        f.rank        || undefined,
    drinkStatus: f.drinkStatus || undefined,
    recipeId:    f.recipeId    || undefined,
  }
}

function applyFilters() { winesStore.fetchWines(buildFilters()) }

function clearFilters() {
  Object.assign(f, { name:'', domain:'', appellation:'', cepages:[], colors:[], year:'', rank:'', drinkStatus:'', recipeId: null })
  pairingSearch.value = ''; selectedRecipeName.value = ''
  applyFilters()
}

let textTimer = null
watch([() => f.name, () => f.domain, () => f.appellation], () => {
  clearTimeout(textTimer); textTimer = setTimeout(applyFilters, 300)
})
watch(() => f.colors,  applyFilters, { deep: true })
watch(() => f.cepages, applyFilters, { deep: true })

async function addToCellar(wineId) {
  try { await cellarStore.addWine(wineId) }
  catch (err) { error.value = err.message }
}

async function removeFromCellar(wineId) { await cellarStore.removeWine(wineId) }

async function handleCreate(data) {
  error.value = ''
  try {
    await winesStore.createWine(data); showForm.value = false
    successMsg.value = 'Wine added!'
    setTimeout(() => { successMsg.value = '' }, 3000)
  } catch (err) { error.value = err.message; throw err }
}

function openDrinkModal(wineId) {
  const wine = winesStore.wines.find(w => w.id === wineId)
  drinkTarget.value = wine ? { id: wine.id, name: `${wine.name} ${wine.year}` } : { id: wineId, name: '' }
}

async function confirmDrink(form) {
  await cellarStore.drinkBottle(drinkTarget.value.id, form)
  drinkTarget.value = null; successMsg.value = 'Bottle recorded!'
  setTimeout(() => { successMsg.value = '' }, 3000)
}

onMounted(() => { winesStore.fetchWines(); cellarStore.fetchCellar() })
</script>
