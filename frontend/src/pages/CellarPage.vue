<template>
  <div>
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h2 class="fw-bold mb-0">My Cellar</h2>
      <span class="badge bg-secondary fs-6">
        {{ cellarStore.items.length }} wine{{ cellarStore.items.length !== 1 ? 's' : '' }}
      </span>
    </div>

    <AlertMessage :message="cellarStore.error" @dismiss="cellarStore.error = null" />
    <LoadingSpinner v-if="cellarStore.loading" />

    <template v-else>
      <!-- ── Summary panels — clickable status filters ──────────────────── -->
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
            style="background:#d1e7dd"
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
            style="background:#fff3cd"
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
            style="background:#f8d7da"
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

      <!-- ── Filter panel ────────────────────────────────────────────────── -->
      <div class="card border-0 bg-light mb-3">
        <div class="card-body py-2 px-3">

          <!-- Row 1: Name / Domain / Appellation -->
          <div class="row g-2 mb-2">
            <div class="col-12 col-sm-4">
              <label class="form-label small mb-1">Name</label>
              <input v-model="f.name" type="text" class="form-control form-control-sm" placeholder="Wine name" />
            </div>
            <div class="col-12 col-sm-4">
              <label class="form-label small mb-1">Domain</label>
              <input v-model="f.domain" type="text" class="form-control form-control-sm" placeholder="Producer / domain" />
            </div>
            <div class="col-12 col-sm-4">
              <label class="form-label small mb-1">Appellation</label>
              <input v-model="f.appellation" type="text" class="form-control form-control-sm" placeholder="e.g. Pauillac" />
            </div>
          </div>

          <!-- Row 2: Cépage -->
          <div class="mb-2">
            <label class="form-label small mb-1">Cépage</label>
            <CepageSelect v-model="f.cepages" />
          </div>

          <!-- Row 3: Color multi-select -->
          <div class="mb-2">
            <label class="form-label small mb-1">Color</label>
            <WineColorPicker v-model="f.colors" />
          </div>

          <!-- Row 4: Vintage / Rank / Status / Drink window -->
          <div class="d-flex flex-wrap gap-2 align-items-center">
            <input
              v-model="f.year"
              type="number"
              class="form-control form-control-sm"
              placeholder="Vintage"
              style="width:90px"
              min="1900" max="2100"
            />
            <select v-model="f.rank" class="form-select form-select-sm" style="width:105px">
              <option value="">All ranks</option>
              <option value="5">★★★★★</option>
              <option value="4">★★★★</option>
              <option value="3">★★★</option>
              <option value="2">★★</option>
              <option value="1">★</option>
            </select>
            <div class="form-check mb-0">
              <input class="form-check-input" type="checkbox" id="thisYearCheck" v-model="f.thisYear" />
              <label class="form-check-label small" for="thisYearCheck">Drink window: this year</label>
            </div>
            <!-- Status dropdown (also controllable from summary panels above) -->
            <select v-model="activeStatus" class="form-select form-select-sm ms-auto" style="width:135px">
              <option value="">All statuses</option>
              <option value="young">Too Young</option>
              <option value="ready">Ready</option>
              <option value="soon">Drink Soon</option>
              <option value="past">Past Peak</option>
            </select>
          </div>

          <!-- Active-filter clear -->
          <div v-if="hasActiveFilters" class="mt-2">
            <button class="btn btn-sm btn-link p-0 text-secondary" style="font-size:.8rem" @click="clearFilters">
              ✕ Clear all filters
            </button>
          </div>
        </div>
      </div>

      <!-- ── Wine table ──────────────────────────────────────────────────── -->
      <div v-if="filteredItems.length === 0" class="text-center py-4 text-muted">
        <div style="font-size:3rem">🍾</div>
        <p class="mt-2 fs-5">
          {{ cellarStore.items.length === 0 ? 'Your cellar is empty.' : 'No results for your filters.' }}
        </p>
        <p v-if="cellarStore.items.length === 0">
          <router-link to="/wines" class="btn btn-sm text-white" style="background-color:#4a1020">Browse Wines</router-link>
        </p>
      </div>

      <div v-else class="table-responsive">
        <table class="table table-hover align-middle">
          <thead class="table-light">
            <tr>
              <th class="sortable" @click="setSort('name')">
                Wine <SortIcon :active="sort.by === 'name'" :dir="sort.dir" />
              </th>
              <th class="sortable" @click="setSort('domain')">
                Domain <SortIcon :active="sort.by === 'domain'" :dir="sort.dir" />
              </th>
              <th class="sortable" @click="setSort('year')">
                Year <SortIcon :active="sort.by === 'year'" :dir="sort.dir" />
              </th>
              <th class="sortable" @click="setSort('rank')">
                Rank <SortIcon :active="sort.by === 'rank'" :dir="sort.dir" />
              </th>
              <th>Status</th>
              <th>Drink Window</th>
              <th class="text-center sortable" @click="setSort('bottles')">
                Bottles <SortIcon :active="sort.by === 'bottles'" :dir="sort.dir" />
              </th>
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
                  <span class="badge" :style="colorBadgeStyle(item.wine.color)" style="font-size:.65rem">{{ item.wine.color }}</span>
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
                    style="width:30px;height:30px;padding:0"
                    @click="cellarStore.decrement(item.wineId)"
                    data-testid="decrement-btn"
                    title="Remove one bottle"
                  >−</button>
                  <span class="fw-bold mx-1" style="min-width:24px;text-align:center" data-testid="bottle-count">
                    {{ item.bottleCount }}
                  </span>
                  <button
                    class="btn btn-outline-secondary btn-sm"
                    style="width:30px;height:30px;padding:0"
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
                    style="background-color:#722f37"
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

    <!-- ── Tasting history ─────────────────────────────────────────────── -->
    <div class="mt-5">
      <div class="d-flex align-items-center gap-3 mb-3 flex-wrap">
        <h4 class="fw-bold mb-0">Tasting History</h4>
        <div style="min-width:200px;max-width:320px" class="flex-grow-1">
          <input
            v-model="historySearch"
            type="text"
            class="form-control form-control-sm"
            placeholder="Search: wine, meal, recipe…"
          />
        </div>
      </div>

      <div v-if="cellarStore.history.length === 0" class="text-center py-4 text-muted">
        <div style="font-size:2rem">📖</div>
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
                <td class="small text-muted" style="max-width:180px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap">
                  {{ record.tastingNote || '—' }}
                </td>
                <td class="small text-muted" style="max-width:140px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap">
                  {{ record.recipeName || record.mealNote || '—' }}
                </td>
                <td class="text-warning text-nowrap">{{ record.pairingRating ? '★'.repeat(record.pairingRating) : '—' }}</td>
                <td class="small text-muted" style="max-width:160px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap">
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
import { ref, reactive, computed, onMounted } from 'vue'
import { useCellarStore } from '@/stores/cellar'
import DrinkModal      from '@/components/DrinkModal.vue'
import WineColorPicker from '@/components/WineColorPicker.vue'
import CepageSelect    from '@/components/CepageSelect.vue'
import LoadingSpinner  from '@/components/LoadingSpinner.vue'
import AlertMessage    from '@/components/AlertMessage.vue'
import { getDrinkStatus, COLOR_STYLES } from '@/utils/drinkStatus'

// ── SortIcon (inline component — no separate file needed) ─────────────────
const SortIcon = {
  props: { active: Boolean, dir: String },
  template: `
    <span class="sort-icon ms-1" :class="active ? 'text-dark' : 'text-muted opacity-50'" style="font-size:.8em">
      <template v-if="active">{{ dir === 'asc' ? '↑' : '↓' }}</template>
      <template v-else>↕</template>
    </span>
  `
}

const cellarStore  = useCellarStore()
const CURRENT_YEAR = new Date().getFullYear()

// ── Filter state ─────────────────────────────────────────────────────────
const f = reactive({
  name:        '',
  domain:      '',
  appellation: '',
  cepages:     [],
  colors:      [],
  year:        '',
  rank:        '',
  thisYear:    false,
})

const activeStatus  = ref('')
const historySearch = ref('')
const drinkTarget   = ref(null)

const STATUS_LABEL = { ready: 'Ready', soon: 'Drink Soon', past: 'Past Peak', young: 'Too Young' }

function setStatus(status) {
  activeStatus.value = activeStatus.value === status ? '' : status
}

const hasActiveFilters = computed(() =>
  !!f.name || !!f.domain || !!f.appellation ||
  f.cepages.length > 0 || f.colors.length > 0 ||
  !!f.year || !!f.rank || f.thisYear ||
  !!activeStatus.value || !!historySearch.value.trim()
)

function clearFilters() {
  Object.assign(f, { name: '', domain: '', appellation: '', cepages: [], colors: [], year: '', rank: '', thisYear: false })
  activeStatus.value  = ''
  historySearch.value = ''
}

// ── Sorting ─────────────────────────────────────────────────────────────
const sort = reactive({ by: 'name', dir: 'asc' })

function setSort(col) {
  if (sort.by === col) {
    sort.dir = sort.dir === 'asc' ? 'desc' : 'asc'
  } else {
    sort.by  = col
    sort.dir = 'asc'
  }
}

// ── Cellar wine list (client-side filter + sort) ──────────────────────────
const filteredItems = computed(() => {
  let items = [...cellarStore.items]

  if (f.name.trim()) {
    const q = f.name.toLowerCase()
    items = items.filter(i => i.wine.name.toLowerCase().includes(q))
  }

  if (f.domain.trim()) {
    const q = f.domain.toLowerCase()
    items = items.filter(i => i.wine.domain.toLowerCase().includes(q))
  }

  if (f.appellation.trim()) {
    const q = f.appellation.toLowerCase()
    items = items.filter(i => i.wine.appellation?.toLowerCase().includes(q))
  }

  if (f.cepages.length > 0) {
    const lc = f.cepages.map(c => c.toLowerCase())
    items = items.filter(i =>
      (i.wine.cepages || []).some(c => lc.includes(c.name.toLowerCase()))
    )
  }

  if (f.colors.length > 0) {
    items = items.filter(i => i.wine.color && f.colors.includes(i.wine.color))
  }

  if (f.year) {
    items = items.filter(i => String(i.wine.year) === f.year)
  }

  if (f.rank) {
    items = items.filter(i => String(i.wine.rank) === f.rank)
  }

  if (f.thisYear) {
    items = items.filter(i => {
      const from = i.wine.drinkFromYear
      const to   = i.wine.drinkToYear
      if (!from && !to) return false
      if (from && to)   return CURRENT_YEAR >= from && CURRENT_YEAR <= to
      if (from)         return CURRENT_YEAR >= from
      return CURRENT_YEAR <= to
    })
  }

  if (activeStatus.value) {
    items = items.filter(i => {
      const s = getDrinkStatus(i.wine)
      return s?.label === STATUS_LABEL[activeStatus.value]
    })
  }

  // ── Sort ────────────────────────────────────────────────────────────────
  const dir = sort.dir === 'asc' ? 1 : -1
  items.sort((a, b) => {
    switch (sort.by) {
      case 'name':    return dir * a.wine.name.localeCompare(b.wine.name)
      case 'domain':  return dir * a.wine.domain.localeCompare(b.wine.domain)
      case 'year':    return dir * (a.wine.year - b.wine.year)
      case 'rank':    return dir * (a.wine.rank - b.wine.rank)
      case 'bottles': return dir * (a.bottleCount - b.bottleCount)
      default:        return 0
    }
  })

  return items
})

// ── Tasting history ───────────────────────────────────────────────────────
const filteredHistory = computed(() => {
  let records = cellarStore.history

  if (activeStatus.value) {
    records = records.filter(r => {
      const s = getDrinkStatus({ drinkFromYear: r.wineDrinkFromYear, drinkToYear: r.wineDrinkToYear })
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

// ── Summary stats ─────────────────────────────────────────────────────────
const stats = computed(() => {
  const totalBottles = cellarStore.items.reduce((sum, i) => sum + i.bottleCount, 0)
  let ready = 0, soon = 0, past = 0
  for (const item of cellarStore.items) {
    const s = getDrinkStatus(item.wine)
    if (!s) continue
    if (s.label === 'Ready')     ready++
    else if (s.label === 'Drink Soon') soon++
    else if (s.label === 'Past Peak')  past++
  }
  return { totalBottles, ready, soon, past }
})

// ── Helpers ───────────────────────────────────────────────────────────────
function getStatus(wine)     { return getDrinkStatus(wine) }
function colorBadgeStyle(color) {
  const s = COLOR_STYLES[color]
  return s ? `background-color:${s.bg};color:${s.text}` : 'background-color:#6c757d;color:white'
}
function stars(rank) { return '★'.repeat(rank) + '☆'.repeat(5 - rank) }
function formatDate(iso) {
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}
function openDrinkModal(item) { drinkTarget.value = item }
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
  box-shadow: 0 4px 12px rgba(0,0,0,.12) !important;
}
.panel-card.is-active {
  transform: translateY(-2px);
  box-shadow: 0 0 0 2px rgba(0,0,0,.35), 0 4px 12px rgba(0,0,0,.15) !important;
}
.sortable {
  cursor: pointer;
  user-select: none;
  white-space: nowrap;
}
.sortable:hover { background-color: rgba(0,0,0,.04); }
</style>
