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

    <!-- ── Filter panel ─────────────────────────────────────────────────── -->
    <div class="card border-0 bg-light mb-3">
      <div class="card-body py-2 px-3">

        <!-- Row 1: Name / Domain / Appellation -->
        <div class="row g-2 mb-2">
          <div class="col-12 col-sm-4">
            <label class="form-label small mb-1">Name</label>
            <input
              v-model="f.name"
              type="text"
              class="form-control form-control-sm"
              placeholder="e.g. Château Margaux"
            />
          </div>
          <div class="col-12 col-sm-4">
            <label class="form-label small mb-1">Domain</label>
            <input
              v-model="f.domain"
              type="text"
              class="form-control form-control-sm"
              placeholder="e.g. Domaine de la Romanée-Conti"
            />
          </div>
          <div class="col-12 col-sm-4">
            <label class="form-label small mb-1">Appellation</label>
            <input
              v-model="f.appellation"
              type="text"
              class="form-control form-control-sm"
              placeholder="e.g. Pauillac"
            />
          </div>
        </div>

        <!-- Row 2: Cépage -->
        <div class="row g-2 mb-2">
          <div class="col-12 col-sm-6">
            <label class="form-label small mb-1">Cépage</label>
            <CepageSelect v-model="f.cepages" />
          </div>
          <div class="col-12 col-sm-6">
            <label class="form-label small mb-1">Pairing</label>
            <!-- Searchable recipe dropdown -->
            <div class="position-relative" data-pairing-picker>
              <input
                v-model="pairingSearch"
                type="text"
                class="form-control form-control-sm"
                :placeholder="f.recipeId ? selectedRecipeName : 'Search a recipe to filter by pairing…'"
                @input="onPairingInput"
                @focus="showPairingDropdown = true"
                @blur="hidePairingDropdown"
              />
              <button
                v-if="f.recipeId"
                class="btn btn-sm btn-link position-absolute top-50 end-0 translate-middle-y pe-2 py-0 text-muted"
                style="font-size:1rem;"
                type="button"
                @click.prevent="clearPairing"
                title="Clear pairing filter"
              >×</button>
              <ul
                v-if="showPairingDropdown && pairingResults.length"
                class="list-group position-absolute w-100 shadow-sm"
                style="z-index:300;max-height:180px;overflow-y:auto;"
              >
                <li
                  v-for="r in pairingResults"
                  :key="r.id"
                  class="list-group-item list-group-item-action py-1 small"
                  @mousedown.prevent="selectPairing(r)"
                >
                  {{ r.name }}
                  <span class="text-muted ms-1">{{ r.recipeType }}</span>
                </li>
              </ul>
            </div>
          </div>
        </div>

        <!-- Row 3: Color multi-select -->
        <div class="mb-2">
          <label class="form-label small mb-1">Color</label>
          <WineColorPicker v-model="f.colors" />
        </div>

        <!-- Row 4: Vintage / Rank / Drink status -->
        <div class="d-flex flex-wrap gap-2 align-items-center">
          <input
            v-model="f.year"
            type="number"
            class="form-control form-control-sm"
            placeholder="Vintage"
            style="width:95px"
            min="1900" max="2100"
            @change="applyFilters"
          />
          <select v-model="f.rank" class="form-select form-select-sm" style="width:110px" @change="applyFilters">
            <option value="">All ranks</option>
            <option value="5">★★★★★</option>
            <option value="4">★★★★</option>
            <option value="3">★★★</option>
            <option value="2">★★</option>
            <option value="1">★</option>
          </select>
          <select v-model="f.drinkStatus" class="form-select form-select-sm" style="width:auto" @change="applyFilters">
            <option v-for="opt in DRINK_STATUS_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>

          <span v-if="hasActiveFilters" class="ms-auto">
            <button class="btn btn-sm btn-link p-0 text-secondary" style="font-size:.8rem" @click="clearFilters">
              ✕ Clear all
            </button>
          </span>
        </div>

      </div>
    </div>

    <LoadingSpinner v-if="winesStore.loading" />

    <template v-else>
      <p class="text-muted small mb-3">
        {{ winesStore.wines.length }} wine{{ winesStore.wines.length !== 1 ? 's' : '' }} found
      </p>

      <div v-if="winesStore.wines.length === 0" class="text-center py-5 text-muted">
        <div style="font-size:3rem">🍷</div>
        <p class="mt-2">No wines found. Try adjusting your filters or add a new wine.</p>
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
import { DRINK_STATUS_OPTIONS } from '@/utils/drinkStatus'

const winesStore  = useWinesStore()
const cellarStore = useCellarStore()

// ── Filter state ──────────────────────────────────────────────────────────────
const f = reactive({
  name:        '',
  domain:      '',
  appellation: '',
  cepages:     [],    // array of strings → multi-select
  colors:      [],    // array of strings → multi-select
  year:        '',
  rank:        '',
  drinkStatus: '',
  recipeId:    null,
})

const showForm   = ref(false)
const error      = ref('')
const successMsg = ref('')
const drinkTarget = ref(null)

// ── Pairing (recipe) search ──────────────────────────────────────────────────
const pairingSearch      = ref('')
const selectedRecipeName = ref('')
const pairingResults     = ref([])
const showPairingDropdown = ref(false)
let pairingTimer = null

function onPairingInput() {
  clearTimeout(pairingTimer)
  f.recipeId = null
  selectedRecipeName.value = ''
  pairingTimer = setTimeout(async () => {
    if (!pairingSearch.value.trim()) { pairingResults.value = []; return }
    try {
      pairingResults.value = (await recipeService.getAll(pairingSearch.value)).slice(0, 8)
    } catch { pairingResults.value = [] }
  }, 250)
}

function selectPairing(recipe) {
  f.recipeId           = recipe.id
  selectedRecipeName.value = recipe.name
  pairingSearch.value  = recipe.name
  pairingResults.value = []
  showPairingDropdown.value = false
  applyFilters()
}

function clearPairing() {
  f.recipeId           = null
  selectedRecipeName.value = ''
  pairingSearch.value  = ''
  pairingResults.value = []
  applyFilters()
}

function hidePairingDropdown() {
  setTimeout(() => { showPairingDropdown.value = false }, 150)
}

// ── Filter helpers ─────────────────────────────────────────────────────────
const hasActiveFilters = computed(() =>
  !!f.name || !!f.domain || !!f.appellation ||
  f.cepages.length > 0 || f.colors.length > 0 ||
  !!f.year || !!f.rank || !!f.drinkStatus || !!f.recipeId
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

function applyFilters() {
  winesStore.fetchWines(buildFilters())
}

function clearFilters() {
  Object.assign(f, { name: '', domain: '', appellation: '', cepages: [], colors: [], year: '', rank: '', drinkStatus: '', recipeId: null })
  pairingSearch.value = ''
  selectedRecipeName.value = ''
  applyFilters()
}

// Debounce text inputs
let textTimer = null
watch([() => f.name, () => f.domain, () => f.appellation], () => {
  clearTimeout(textTimer)
  textTimer = setTimeout(applyFilters, 300)
})

// Immediate filter for multi-select pickers (colors / cepages)
watch(() => f.colors, applyFilters, { deep: true })
watch(() => f.cepages, applyFilters, { deep: true })

// ── Cellar actions ─────────────────────────────────────────────────────────
async function addToCellar(wineId) {
  try { await cellarStore.addWine(wineId) }
  catch (err) { error.value = err.message }
}

async function removeFromCellar(wineId) {
  await cellarStore.removeWine(wineId)
}

// ── Create wine ────────────────────────────────────────────────────────────
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

// ── Drink modal ────────────────────────────────────────────────────────────
function openDrinkModal(wineId) {
  const wine = winesStore.wines.find(w => w.id === wineId)
  drinkTarget.value = wine
    ? { id: wine.id, name: `${wine.name} ${wine.year}` }
    : { id: wineId, name: '' }
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
