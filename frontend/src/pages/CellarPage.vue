<template>
  <div>
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2 class="fw-bold mb-0">My Cellar</h2>
      <span class="badge bg-secondary fs-6">{{ cellarStore.items.length }} wine{{ cellarStore.items.length !== 1 ? 's' : '' }}</span>
    </div>

    <AlertMessage :message="cellarStore.error" @dismiss="cellarStore.error = null" />
    <LoadingSpinner v-if="cellarStore.loading" />

    <template v-else>
      <!-- Summary panels — clickable status filters -->
      <div v-if="cellarStore.items.length > 0" class="row g-3 mb-4">
        <div class="col-6 col-sm-3">
          <div
            :class="['card text-center border-0 bg-light h-100 panel-card', { 'is-active': activeStatus === '' }]"
            @click="activeStatus = ''"
            title="Show all wines"
          >
            <div class="card-body py-2">
              <div class="fw-bold fs-4">{{ stats.totalBottles }}</div>
              <div class="text-muted small">Total bottles</div>
            </div>
          </div>
        </div>
        <div class="col-6 col-sm-3">
          <div
            :class="['card text-center border-0 h-100 panel-card', { 'is-active': activeStatus === 'ready' }]"
            style="background: #d1e7dd;"
            @click="setStatus('ready')"
            title="Filter: Ready to drink"
          >
            <div class="card-body py-2">
              <div class="fw-bold fs-4 text-success">{{ stats.ready }}</div>
              <div class="text-muted small">Ready to drink</div>
            </div>
          </div>
        </div>
        <div class="col-6 col-sm-3">
          <div
            :class="['card text-center border-0 h-100 panel-card', { 'is-active': activeStatus === 'soon' }]"
            style="background: #fff3cd;"
            @click="setStatus('soon')"
            title="Filter: Drink Soon"
          >
            <div class="card-body py-2">
              <div class="fw-bold fs-4 text-warning">{{ stats.soon }}</div>
              <div class="text-muted small">Drink soon</div>
            </div>
          </div>
        </div>
        <div class="col-6 col-sm-3">
          <div
            :class="['card text-center border-0 h-100 panel-card', { 'is-active': activeStatus === 'past' }]"
            style="background: #f8d7da;"
            @click="setStatus('past')"
            title="Filter: Past Peak"
          >
            <div class="card-body py-2">
              <div class="fw-bold fs-4 text-danger">{{ stats.past }}</div>
              <div class="text-muted small">Past peak</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Search bar -->
      <div class="mb-2">
        <SearchBar v-model="search" placeholder="Search by wine, domain, region..." />
      </div>

      <!-- Filter row -->
      <div class="row g-2 mb-3 align-items-center">
        <div class="col-auto">
          <input
            v-model="filterYear"
            type="number"
            class="form-control form-control-sm"
            placeholder="Vintage"
            style="width: 100px;"
            min="1900"
            max="2100"
          />
        </div>
        <div class="col-auto">
          <select v-model="filterRank" class="form-select form-select-sm" style="width: 105px;">
            <option value="">All ranks</option>
            <option value="5">★★★★★</option>
            <option value="4">★★★★</option>
            <option value="3">★★★</option>
            <option value="2">★★</option>
            <option value="1">★</option>
          </select>
        </div>
        <div class="col-auto">
          <div class="form-check mb-0">
            <input class="form-check-input" type="checkbox" id="thisYearCheck" v-model="filterThisYear" />
            <label class="form-check-label small" for="thisYearCheck">Drink window: this year</label>
          </div>
        </div>
        <!-- Status dropdown gives access to "Too Young" not shown as a top panel -->
        <div class="col-auto ms-auto">
          <select v-model="activeStatus" class="form-select form-select-sm" style="width: 135px;">
            <option value="">All statuses</option>
            <option value="young">Too Young</option>
            <option value="ready">Ready</option>
            <option value="soon">Drink Soon</option>
            <option value="past">Past Peak</option>
          </select>
        </div>
      </div>

      <!-- Active-filter indicator -->
      <div v-if="hasActiveFilters" class="mb-2 d-flex align-items-center gap-2">
        <span class="text-muted small">Filters active</span>
        <button class="btn btn-sm btn-outline-secondary py-0 px-2" style="font-size: 0.75rem;" @click="clearFilters">
          Clear all
        </button>
      </div>

      <!-- Wine list -->
      <div v-if="filteredItems.length === 0" class="text-center py-4 text-muted">
        <div style="font-size: 3rem;">🍾</div>
        <p class="mt-2 fs-5">{{ cellarStore.items.length === 0 ? 'Your cellar is empty.' : 'No results for your search.' }}</p>
        <p v-if="cellarStore.items.length === 0">
          <router-link to="/wines" class="btn btn-sm text-white" style="background-color: #4a1020;">Browse Wines</router-link>
        </p>
      </div>

      <div v-else class="table-responsive">
        <table class="table table-hover align-middle">
          <thead class="table-light">
            <tr>
              <th>Wine</th>
              <th>Domain</th>
              <th>Year</th>
              <th>Rank</th>
              <th>Status</th>
              <th>Drink Window</th>
              <th class="text-center">Bottles</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in filteredItems" :key="item.wineId" data-testid="cellar-item">
              <td class="fw-semibold">
                <router-link :to="`/wines/${item.wineId}`" class="text-decoration-none text-dark">
                  {{ item.wine.name }}
                </router-link>
                <div v-if="item.wine.color" class="mt-1">
                  <span class="badge" :style="colorBadgeStyle(item.wine.color)" style="font-size: 0.65rem;">{{ item.wine.color }}</span>
                </div>
              </td>
              <td class="text-muted small">{{ item.wine.domain }}</td>
              <td>{{ item.wine.year }}</td>
              <td class="text-warning small">{{ stars(item.wine.rank) }}</td>
              <td>
                <span v-if="getStatus(item.wine)" class="badge" :class="`bg-${getStatus(item.wine).bg}`">
                  {{ getStatus(item.wine).label }}
                </span>
                <span v-else class="text-muted small">—</span>
              </td>
              <td class="small text-muted">
                <template v-if="item.wine.drinkFromYear || item.wine.drinkToYear">
                  {{ item.wine.drinkFromYear || '?' }} – {{ item.wine.drinkToYear || '?' }}
                </template>
                <span v-else>—</span>
              </td>
              <td class="text-center">
                <div class="d-flex align-items-center justify-content-center gap-1">
                  <button
                    class="btn btn-outline-secondary btn-sm"
                    style="width: 30px; height: 30px; padding: 0;"
                    @click="cellarStore.decrement(item.wineId)"
                    data-testid="decrement-btn"
                    title="Remove one bottle"
                  >−</button>
                  <span class="fw-bold mx-1" style="min-width: 24px; text-align: center;" data-testid="bottle-count">
                    {{ item.bottleCount }}
                  </span>
                  <button
                    class="btn btn-outline-secondary btn-sm"
                    style="width: 30px; height: 30px; padding: 0;"
                    @click="cellarStore.increment(item.wineId)"
                    data-testid="increment-btn"
                    title="Add one bottle"
                  >+</button>
                </div>
              </td>
              <td>
                <div class="d-flex gap-1">
                  <button
                    class="btn btn-sm text-white"
                    style="background-color: #722f37;"
                    data-testid="drink-btn"
                    @click="openDrinkModal(item)"
                  >Drink</button>
                  <router-link :to="`/wines/${item.wineId}`" class="btn btn-outline-secondary btn-sm">Details</router-link>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <!-- Drink modal -->
    <DrinkModal
      :show="!!drinkTarget"
      :wine-id="drinkTarget?.wineId"
      :wine-name="drinkTarget ? `${drinkTarget.wine.name} ${drinkTarget.wine.year}` : ''"
      @confirm="confirmDrink"
      @cancel="drinkTarget = null"
    />

    <!-- Tasting history -->
    <div class="mt-5">
      <div class="d-flex align-items-center gap-3 mb-3 flex-wrap">
        <h4 class="fw-bold mb-0">Tasting History</h4>
        <div style="min-width: 200px; max-width: 320px;" class="flex-grow-1">
          <input
            v-model="historySearch"
            type="text"
            class="form-control form-control-sm"
            placeholder="Search: wine, meal, recipe..."
          />
        </div>
      </div>

      <div v-if="cellarStore.history.length === 0" class="text-center py-4 text-muted">
        <div style="font-size: 2rem;">📖</div>
        <p class="mt-2">No tasting records yet. Drink a bottle to start your history.</p>
      </div>

      <template v-else>
        <p v-if="filteredHistory.length < cellarStore.history.length" class="text-muted small mb-2">
          Showing {{ filteredHistory.length }} of {{ cellarStore.history.length }} records
        </p>

        <div v-if="filteredHistory.length === 0" class="text-center py-3 text-muted">
          <p class="mb-1">No tasting records match the current filter.</p>
          <button class="btn btn-sm btn-link p-0" @click="clearFilters">Clear filters</button>
        </div>

        <div v-else class="table-responsive">
          <table class="table table-hover align-middle">
            <thead class="table-light">
              <tr>
                <th>Wine</th>
                <th>Date</th>
                <th>Rating ★</th>
                <th>Tasting note</th>
                <th>Meal</th>
                <th>Pairing ★</th>
                <th>Pairing note</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="record in filteredHistory" :key="record.id" data-testid="history-item">
                <td class="fw-semibold">
                  <router-link :to="`/wines/${record.wineId}`" class="text-decoration-none text-dark">
                    {{ record.wineName }}
                  </router-link>
                  <div class="text-muted small">{{ record.wineYear }}</div>
                </td>
                <td class="small text-nowrap">{{ formatDate(record.consumedAt) }}</td>
                <td class="text-warning text-nowrap">{{ record.rating ? '★'.repeat(record.rating) : '—' }}</td>
                <td class="small text-muted" style="max-width: 180px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                  {{ record.tastingNote || '—' }}
                </td>
                <td class="small text-muted" style="max-width: 140px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                  {{ record.recipeName || record.mealNote || '—' }}
                </td>
                <td class="text-warning text-nowrap">{{ record.pairingRating ? '★'.repeat(record.pairingRating) : '—' }}</td>
                <td class="small text-muted" style="max-width: 160px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                  {{ record.pairingNote || '—' }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useCellarStore } from '@/stores/cellar'
import DrinkModal from '@/components/DrinkModal.vue'
import SearchBar from '@/components/SearchBar.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import AlertMessage from '@/components/AlertMessage.vue'
import { getDrinkStatus, COLOR_STYLES } from '@/utils/drinkStatus'

const cellarStore = useCellarStore()
const CURRENT_YEAR = new Date().getFullYear()

// ── Filter state ────────────────────────────────────────────────────────────
const search = ref('')
const activeStatus = ref('')   // '' | 'ready' | 'soon' | 'past' | 'young'
const filterYear = ref('')
const filterRank = ref('')
const filterThisYear = ref(false)
const historySearch = ref('')
const drinkTarget = ref(null)

// Label lookup shared by wine list and history filters
const STATUS_LABEL = { ready: 'Ready', soon: 'Drink Soon', past: 'Past Peak', young: 'Too Young' }

// Clicking an already-active panel toggles it back to "All"
function setStatus(status) {
  activeStatus.value = activeStatus.value === status ? '' : status
}

const hasActiveFilters = computed(() =>
  !!search.value.trim() ||
  !!activeStatus.value ||
  !!filterYear.value ||
  !!filterRank.value ||
  filterThisYear.value ||
  !!historySearch.value.trim()
)

function clearFilters() {
  search.value = ''
  activeStatus.value = ''
  filterYear.value = ''
  filterRank.value = ''
  filterThisYear.value = false
  historySearch.value = ''
}

// ── Cellar wine list ────────────────────────────────────────────────────────
const filteredItems = computed(() => {
  let items = cellarStore.items

  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    items = items.filter(i =>
      i.wine.name.toLowerCase().includes(q) ||
      i.wine.domain.toLowerCase().includes(q) ||
      String(i.wine.year).includes(q) ||
      i.wine.color?.toLowerCase().includes(q) ||
      i.wine.country?.toLowerCase().includes(q) ||
      i.wine.region?.toLowerCase().includes(q)
    )
  }

  if (filterYear.value) {
    items = items.filter(i => String(i.wine.year) === filterYear.value)
  }

  if (filterRank.value) {
    items = items.filter(i => String(i.wine.rank) === filterRank.value)
  }

  if (filterThisYear.value) {
    items = items.filter(i => {
      const from = i.wine.drinkFromYear
      const to = i.wine.drinkToYear
      if (!from && !to) return false
      if (from && to) return CURRENT_YEAR >= from && CURRENT_YEAR <= to
      if (from) return CURRENT_YEAR >= from
      return CURRENT_YEAR <= to
    })
  }

  if (activeStatus.value) {
    items = items.filter(i => {
      const s = getDrinkStatus(i.wine)
      return s?.label === STATUS_LABEL[activeStatus.value]
    })
  }

  return items
})

// ── Tasting history ─────────────────────────────────────────────────────────
// Status filter uses wineDrinkFromYear/wineDrinkToYear returned by the API,
// so it works even for wines that have since been removed from the cellar.
const filteredHistory = computed(() => {
  let records = cellarStore.history

  if (activeStatus.value) {
    records = records.filter(r => {
      const s = getDrinkStatus({
        drinkFromYear: r.wineDrinkFromYear,
        drinkToYear: r.wineDrinkToYear,
      })
      return s?.label === STATUS_LABEL[activeStatus.value]
    })
  }

  const q = historySearch.value.trim().toLowerCase()
  if (q) {
    records = records.filter(r =>
      r.wineName.toLowerCase().includes(q) ||
      r.wineDomain.toLowerCase().includes(q) ||
      String(r.wineYear).includes(q) ||
      r.recipeName?.toLowerCase().includes(q) ||
      r.mealNote?.toLowerCase().includes(q)
    )
  }

  return records
})

// ── Summary stats (always computed over full cellar, not filtered) ───────────
const stats = computed(() => {
  const totalBottles = cellarStore.items.reduce((sum, i) => sum + i.bottleCount, 0)
  let ready = 0, soon = 0, past = 0
  for (const item of cellarStore.items) {
    const s = getDrinkStatus(item.wine)
    if (!s) continue
    if (s.label === 'Ready') ready++
    else if (s.label === 'Drink Soon') soon++
    else if (s.label === 'Past Peak') past++
  }
  return { totalBottles, ready, soon, past }
})

// ── Helpers ─────────────────────────────────────────────────────────────────
function getStatus(wine) {
  return getDrinkStatus(wine)
}

function colorBadgeStyle(color) {
  const s = COLOR_STYLES[color]
  return s ? `background-color: ${s.bg}; color: ${s.text};` : 'background-color: #6c757d; color: white;'
}

function stars(rank) {
  return '★'.repeat(rank) + '☆'.repeat(5 - rank)
}

function formatDate(iso) {
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}

function openDrinkModal(item) {
  drinkTarget.value = item
}

async function confirmDrink(form) {
  await cellarStore.drinkBottle(drinkTarget.value.wineId, form)
  drinkTarget.value = null
}

onMounted(() => {
  cellarStore.fetchCellar()
  cellarStore.fetchHistory()
})
</script>

<style scoped>
.panel-card {
  cursor: pointer;
  transition: transform 0.15s ease, box-shadow 0.15s ease;
  user-select: none;
}
.panel-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12) !important;
}
.panel-card.is-active {
  transform: translateY(-2px);
  box-shadow: 0 0 0 2px rgba(0, 0, 0, 0.35), 0 4px 12px rgba(0, 0, 0, 0.15) !important;
}
</style>
