<template>
  <div class="space-y-6">

    <PageHeader eyebrow="My Collection" title="My Cellar">
      <span v-if="cellarStore.items.length > 0" class="text-sm text-base-content/40 font-medium self-end pb-0.5">
        {{ cellarStore.items.length }} wine{{ cellarStore.items.length !== 1 ? 's' : '' }}
      </span>
    </PageHeader>

    <AlertMessage :message="cellarStore.error" @dismiss="cellarStore.error = null" />
    <LoadingSpinner v-if="cellarStore.loading" />

    <template v-else>

      <!-- Summary tiles -->
      <div v-if="cellarStore.items.length > 0" class="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <div
          class="stat-tile"
          :class="activeStatus === '' ? 'bg-primary/8 ring-1 ring-primary/25' : 'bg-base-100 shadow-sm hover:-translate-y-0.5'"
          @click="activeStatus = ''"
        >
          <div class="stat-tile-value text-base-content">{{ stats.totalBottles }}</div>
          <div class="stat-tile-label">Total bottles</div>
        </div>
        <div
          class="stat-tile"
          :class="activeStatus === 'ready' ? 'bg-success/10 ring-1 ring-success/30' : 'bg-base-100 shadow-sm hover:-translate-y-0.5'"
          @click="setStatus('ready')"
        >
          <div class="stat-tile-value text-success">{{ stats.ready }}</div>
          <div class="stat-tile-label">Ready to drink</div>
        </div>
        <div
          class="stat-tile"
          :class="activeStatus === 'soon' ? 'bg-warning/10 ring-1 ring-warning/30' : 'bg-base-100 shadow-sm hover:-translate-y-0.5'"
          @click="setStatus('soon')"
        >
          <div class="stat-tile-value text-warning">{{ stats.soon }}</div>
          <div class="stat-tile-label">Drink soon</div>
        </div>
        <div
          class="stat-tile"
          :class="activeStatus === 'past' ? 'bg-error/10 ring-1 ring-error/30' : 'bg-base-100 shadow-sm hover:-translate-y-0.5'"
          @click="setStatus('past')"
        >
          <div class="stat-tile-value text-error">{{ stats.past }}</div>
          <div class="stat-tile-label">Past peak</div>
        </div>
      </div>

      <!-- Filters -->
      <div class="filter-panel">
        <!-- Mobile toggle header -->
        <button
          class="md:hidden w-full flex items-center justify-between text-sm font-semibold text-base-content/70"
          @click="filtersOpen = !filtersOpen"
        >
          <span class="flex items-center gap-2">
            Filters
            <span
              v-if="activeFilterCount > 0"
              class="inline-flex items-center justify-center h-4 min-w-[1rem] px-1 text-[9px] font-bold bg-primary/15 text-primary rounded-full"
            >{{ activeFilterCount }}</span>
          </span>
          <svg
            class="w-4 h-4 text-base-content/30 transition-transform duration-150 shrink-0"
            :class="filtersOpen ? 'rotate-180' : ''"
            viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
            aria-hidden="true"
          >
            <path d="M6 9l6 6 6-6"/>
          </svg>
        </button>

        <!-- Filter fields: always visible md+, toggle on mobile -->
        <div :class="['space-y-3', filtersOpen ? 'mt-3' : 'hidden md:block']">
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <input v-model="f.name" type="text" class="input input-bordered input-sm w-full" placeholder="Name…" />
            <input v-model="f.domain" type="text" class="input input-bordered input-sm w-full" placeholder="Domain…" />
            <input v-model="f.appellation" type="text" class="input input-bordered input-sm w-full" placeholder="Appellation…" />
          </div>

          <div>
            <label class="label-caps mb-1.5 block">Cépage</label>
            <CepageSelect v-model="f.cepages" />
          </div>

          <div>
            <label class="label-caps mb-1.5 block">Color</label>
            <WineColorPicker v-model="f.colors" />
          </div>

          <div class="flex flex-wrap gap-2 items-center">
            <input v-model="f.year" type="number" class="input input-bordered input-sm w-24" placeholder="Vintage" min="1900" max="2100" />
            <select v-model="f.rank" class="select select-bordered select-sm">
              <option value="">All ranks</option>
              <option value="5">★★★★★</option>
              <option value="4">★★★★</option>
              <option value="3">★★★</option>
              <option value="2">★★</option>
              <option value="1">★</option>
            </select>
            <label class="flex items-center gap-2 text-sm cursor-pointer">
              <input v-model="f.thisYear" type="checkbox" class="checkbox checkbox-sm checkbox-primary" />
              This year's window
            </label>
            <select v-model="activeStatus" class="select select-bordered select-sm ml-auto">
              <option value="">All statuses</option>
              <option value="young">Too Young</option>
              <option value="ready">Ready</option>
              <option value="soon">Drink Soon</option>
              <option value="past">Past Peak</option>
            </select>
          </div>

          <div v-if="hasActiveFilters">
            <button class="btn btn-xs btn-ghost" @click="clearFilters">✕ Clear filters</button>
          </div>
        </div>
      </div>

      <!-- Empty state -->
      <EmptyState
        v-if="filteredItems.length === 0"
        icon="🍾"
        :title="cellarStore.items.length === 0 ? 'Your cellar is empty' : 'No results match your filters'"
        :body="cellarStore.items.length === 0
          ? 'Browse the wine catalog and add bottles you own.'
          : 'Try clearing your filters to see all cellar wines.'"
      >
        <router-link v-if="cellarStore.items.length === 0" to="/wines" class="btn btn-primary btn-sm mt-5">
          Browse Wines
        </router-link>
      </EmptyState>

      <template v-else>

        <!-- Sort + count bar (mobile only) -->
        <div class="md:hidden flex items-center justify-between gap-2">
          <span class="text-xs text-base-content/40 font-medium">
            {{ filteredItems.length }} wine{{ filteredItems.length !== 1 ? 's' : '' }}
          </span>
          <div class="flex items-center gap-1.5">
            <span class="text-xs text-base-content/40">Sort:</span>
            <select v-model="sort.by" class="select select-bordered select-xs" @change="sort.dir = 'asc'">
              <option value="name">Name</option>
              <option value="domain">Domain</option>
              <option value="year">Vintage</option>
              <option value="bottles">Bottles</option>
            </select>
            <button
              class="w-7 h-7 rounded-lg border border-base-200 text-xs text-base-content/50 hover:text-primary hover:border-primary/30 transition-colors flex items-center justify-center"
              @click="sort.dir = sort.dir === 'asc' ? 'desc' : 'asc'"
            >{{ sort.dir === 'asc' ? '↑' : '↓' }}</button>
          </div>
        </div>

        <!-- Mobile: card list -->
        <div class="md:hidden space-y-3">
          <div
            v-for="item in filteredItems"
            :key="item.wineId"
            class="bg-base-100 rounded-2xl border border-base-200 overflow-hidden shadow-sm flex"
            data-testid="cellar-item"
          >
            <!-- Color accent bar -->
            <div
              class="w-1.5 shrink-0"
              :style="{ backgroundColor: getWineAccentColor(item.wine.color) }"
              aria-hidden="true"
            ></div>

            <!-- Card body -->
            <div class="flex-1 p-4 min-w-0">
              <!-- Name + details link -->
              <div class="flex items-start gap-2">
                <div class="flex-1 min-w-0">
                  <div class="font-semibold text-base-content leading-snug line-clamp-2">{{ item.wine.name }}</div>
                  <div class="text-xs text-base-content/45 mt-0.5 flex items-center gap-1.5">
                    <span class="truncate">{{ item.wine.domain }}</span>
                    <span v-if="item.wine.year" class="text-base-content/25">·</span>
                    <span v-if="item.wine.year" class="shrink-0">{{ item.wine.year }}</span>
                  </div>
                </div>
                <router-link
                  :to="`/wines/${item.wineId}`"
                  class="shrink-0 p-1 -mr-1 -mt-0.5 text-base-content/20 hover:text-primary transition-colors"
                  aria-label="View wine details"
                >
                  <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M9 18l6-6-6-6"/>
                  </svg>
                </router-link>
              </div>

              <!-- Badges row -->
              <div class="flex items-center flex-wrap gap-1.5 mt-2.5">
                <WineColorBadge v-if="item.wine.color" :color="item.wine.color" />
                <DrinkStatusBadge v-if="getStatus(item.wine)" :status="getStatus(item.wine)" />
                <StarRating v-if="item.wine.rank" :rank="item.wine.rank" class="ml-auto" />
              </div>

              <!-- Controls row -->
              <div class="border-t border-base-200/60 mt-3 pt-3 flex items-center justify-between gap-2">
                <div class="flex items-center gap-2">
                  <button
                    class="w-9 h-9 rounded-xl border border-base-300 text-base-content/60 hover:border-primary/40 hover:text-primary active:scale-95 transition-all flex items-center justify-center font-semibold text-base"
                    @click="cellarStore.decrement(item.wineId)"
                    data-testid="decrement-btn"
                    aria-label="Remove one bottle"
                  >−</button>
                  <span class="font-bold text-sm tabular-nums text-base-content min-w-[1.25rem] text-center" data-testid="bottle-count">{{ item.bottleCount }}</span>
                  <button
                    class="w-9 h-9 rounded-xl border border-base-300 text-base-content/60 hover:border-primary/40 hover:text-primary active:scale-95 transition-all flex items-center justify-center font-semibold text-base"
                    @click="cellarStore.increment(item.wineId)"
                    data-testid="increment-btn"
                    aria-label="Add one bottle"
                  >+</button>
                  <span class="text-xs text-base-content/30">btl{{ item.bottleCount !== 1 ? 's' : '' }}</span>
                </div>
                <button
                  class="btn btn-sm btn-secondary"
                  @click="openDrinkModal(item)"
                  data-testid="drink-btn"
                >Drink</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Desktop: table -->
        <div class="hidden md:block rounded-2xl border border-base-200 overflow-hidden bg-base-100 shadow-sm">
          <div class="overflow-x-auto">
            <table class="table table-sm table-head-light w-full">
              <thead>
                <tr>
                  <th class="cursor-pointer select-none" @click="setSort('name')">
                    Wine <span class="text-[10px]">{{ sortIcon('name') }}</span>
                  </th>
                  <th class="cursor-pointer select-none hidden sm:table-cell" @click="setSort('domain')">
                    Domain <span class="text-[10px]">{{ sortIcon('domain') }}</span>
                  </th>
                  <th class="cursor-pointer select-none" @click="setSort('year')">
                    Year <span class="text-[10px]">{{ sortIcon('year') }}</span>
                  </th>
                  <th class="hidden md:table-cell">Status</th>
                  <th class="hidden lg:table-cell">Window</th>
                  <th class="cursor-pointer select-none text-center" @click="setSort('bottles')">
                    Bottles <span class="text-[10px]">{{ sortIcon('bottles') }}</span>
                  </th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="item in filteredItems"
                  :key="item.wineId"
                  class="border-b border-base-200 last:border-0 hover:bg-base-200/40 transition-colors"
                  data-testid="cellar-item"
                >
                  <td class="font-medium">
                    <router-link :to="`/wines/${item.wineId}`" class="hover:text-primary transition-colors">
                      {{ item.wine.name }}
                    </router-link>
                    <WineColorBadge v-if="item.wine.color" :color="item.wine.color" class="ml-2" />
                  </td>
                  <td class="text-base-content/50 text-sm hidden sm:table-cell">{{ item.wine.domain }}</td>
                  <td class="text-sm">{{ item.wine.year }}</td>
                  <td class="hidden md:table-cell">
                    <DrinkStatusBadge v-if="getStatus(item.wine)" :status="getStatus(item.wine)" />
                    <span v-else class="text-base-content/25 text-xs">—</span>
                  </td>
                  <td class="text-xs text-base-content/40 hidden lg:table-cell whitespace-nowrap">
                    <template v-if="item.wine.drinkFromYear || item.wine.drinkToYear">
                      {{ item.wine.drinkFromYear || '?' }} – {{ item.wine.drinkToYear || '?' }}
                    </template>
                    <span v-else>—</span>
                  </td>
                  <td class="text-center">
                    <div class="flex items-center justify-center gap-1.5">
                      <button
                        class="w-7 h-7 rounded-lg border border-base-300 text-base-content/60 hover:border-primary/40 hover:text-primary transition-colors flex items-center justify-center text-sm"
                        @click="cellarStore.decrement(item.wineId)"
                        data-testid="decrement-btn"
                      >−</button>
                      <span class="font-bold w-6 text-center text-sm tabular-nums" data-testid="bottle-count">{{ item.bottleCount }}</span>
                      <button
                        class="w-7 h-7 rounded-lg border border-base-300 text-base-content/60 hover:border-primary/40 hover:text-primary transition-colors flex items-center justify-center text-sm"
                        @click="cellarStore.increment(item.wineId)"
                        data-testid="increment-btn"
                      >+</button>
                    </div>
                  </td>
                  <td>
                    <div class="flex gap-1.5">
                      <button class="btn btn-xs btn-secondary" @click="openDrinkModal(item)" data-testid="drink-btn">Drink</button>
                      <router-link :to="`/wines/${item.wineId}`" class="btn btn-xs btn-ghost border border-base-200">Details</router-link>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

      </template>

    </template>

    <DrinkModal
      :show="!!drinkTarget"
      :wine-id="drinkTarget?.wineId"
      :wine-name="drinkTarget ? `${drinkTarget.wine.name} ${drinkTarget.wine.year}` : ''"
      @confirm="confirmDrink"
      @cancel="drinkTarget = null"
    />

    <!-- Tasting History -->
    <div class="mt-4 space-y-4">
      <div class="wine-rule"></div>

      <div class="flex items-center gap-4 flex-wrap">
        <h2 class="section-title">Tasting History</h2>
        <input
          v-model="historySearch"
          type="text"
          class="input input-bordered input-sm flex-1 max-w-xs"
          placeholder="Search: wine, meal, recipe…"
        />
      </div>

      <div v-if="cellarStore.history.length === 0" class="empty-state py-12">
        <span class="empty-state-icon">📖</span>
        <h3 class="empty-state-title">No tasting records yet</h3>
        <p class="empty-state-body">Start recording tastings when you drink a bottle.</p>
      </div>

      <template v-else>
        <p v-if="filteredHistory.length < cellarStore.history.length" class="text-xs text-base-content/40">
          Showing {{ filteredHistory.length }} of {{ cellarStore.history.length }} records
        </p>

        <div v-if="filteredHistory.length === 0" class="text-center py-6 text-base-content/40 text-sm">
          No records match.
          <button class="link link-primary ml-1" @click="historySearch = ''">Clear search</button>
        </div>

        <template v-else>
          <!-- Mobile: history cards -->
          <div class="md:hidden space-y-2">
            <div
              v-for="record in filteredHistory"
              :key="record.id"
              class="bg-base-100 rounded-xl border border-base-200 p-3.5 shadow-sm"
              data-testid="history-item"
            >
              <div class="flex items-start justify-between gap-2">
                <div class="min-w-0">
                  <router-link :to="`/wines/${record.wineId}`" class="font-semibold text-sm hover:text-primary transition-colors">
                    {{ record.wineName }}
                  </router-link>
                  <div class="text-xs text-base-content/40 mt-0.5">{{ record.wineYear }}</div>
                </div>
                <span class="text-xs text-base-content/40 whitespace-nowrap shrink-0">{{ formatDate(record.consumedAt) }}</span>
              </div>
              <div class="flex items-center gap-3 mt-2 flex-wrap">
                <span v-if="record.rating" class="text-amber-400 text-sm">{{ '★'.repeat(record.rating) }}</span>
                <span v-else class="text-base-content/30 text-xs">No rating</span>
                <span v-if="record.recipeName || record.mealNote" class="text-xs text-base-content/45 truncate flex-1">
                  with {{ record.recipeName || record.mealNote }}
                </span>
                <span v-if="record.pairingRating" class="text-amber-400 text-xs ml-auto shrink-0">
                  Pairing: {{ '★'.repeat(record.pairingRating) }}
                </span>
              </div>
              <p v-if="record.tastingNote" class="text-xs text-base-content/40 mt-1.5 italic line-clamp-2">
                "{{ record.tastingNote }}"
              </p>
            </div>
          </div>

          <!-- Desktop: history table -->
          <div class="hidden md:block rounded-2xl border border-base-200 overflow-hidden bg-base-100 shadow-sm">
            <div class="overflow-x-auto">
              <table class="table table-sm table-head-light w-full text-sm">
                <thead>
                  <tr>
                    <th>Wine</th>
                    <th>Date</th>
                    <th>Rating</th>
                    <th class="hidden md:table-cell">Note</th>
                    <th class="hidden sm:table-cell">Meal</th>
                    <th>Pairing</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="record in filteredHistory"
                    :key="record.id"
                    class="border-b border-base-200 last:border-0 hover:bg-base-200/40 transition-colors"
                    data-testid="history-item"
                  >
                    <td class="font-medium">
                      <router-link :to="`/wines/${record.wineId}`" class="hover:text-primary transition-colors">{{ record.wineName }}</router-link>
                      <div class="text-[10px] text-base-content/40 tabular-nums">{{ record.wineYear }}</div>
                    </td>
                    <td class="whitespace-nowrap text-base-content/50 text-xs">{{ formatDate(record.consumedAt) }}</td>
                    <td class="text-amber-400 whitespace-nowrap text-sm">{{ record.rating ? '★'.repeat(record.rating) : '—' }}</td>
                    <td class="max-w-[160px] truncate text-base-content/50 hidden md:table-cell text-xs">{{ record.tastingNote || '—' }}</td>
                    <td class="max-w-[120px] truncate text-base-content/50 hidden sm:table-cell text-xs">{{ record.recipeName || record.mealNote || '—' }}</td>
                    <td class="text-amber-400 whitespace-nowrap text-sm">{{ record.pairingRating ? '★'.repeat(record.pairingRating) : '—' }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </template>
      </template>
    </div>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useCellarStore } from '@/stores/cellar'
import DrinkModal       from '@/components/DrinkModal.vue'
import WineColorPicker  from '@/components/WineColorPicker.vue'
import CepageSelect     from '@/components/CepageSelect.vue'
import LoadingSpinner   from '@/components/LoadingSpinner.vue'
import AlertMessage     from '@/components/AlertMessage.vue'
import { getDrinkStatus } from '@/utils/drinkStatus'
import { COLOR_STYLES }   from '@/utils/wineColor'
import PageHeader       from '@/components/ui/PageHeader.vue'
import EmptyState       from '@/components/ui/EmptyState.vue'
import WineColorBadge   from '@/components/ui/WineColorBadge.vue'
import DrinkStatusBadge from '@/components/ui/DrinkStatusBadge.vue'
import StarRating       from '@/components/ui/StarRating.vue'

const cellarStore  = useCellarStore()
const CURRENT_YEAR = new Date().getFullYear()

const f = reactive({ name:'', domain:'', appellation:'', cepages:[], colors:[], year:'', rank:'', thisYear: false })
const activeStatus  = ref('')
const historySearch = ref('')
const drinkTarget   = ref(null)
const sort          = reactive({ by: 'name', dir: 'asc' })
const filtersOpen   = ref(false)

const STATUS_LABEL = { ready: 'Ready', soon: 'Drink Soon', past: 'Past Peak', young: 'Too Young' }

function setStatus(s) { activeStatus.value = activeStatus.value === s ? '' : s }
function setSort(col) {
  sort.dir = sort.by === col ? (sort.dir === 'asc' ? 'desc' : 'asc') : 'asc'
  sort.by  = col
}
function sortIcon(col) {
  if (sort.by !== col) return '↕'
  return sort.dir === 'asc' ? '↑' : '↓'
}

function getWineAccentColor(color) {
  return COLOR_STYLES[color]?.bg ?? '#d4c5b2'
}

const activeFilterCount = computed(() =>
  [f.name, f.domain, f.appellation].filter(Boolean).length +
  (f.cepages.length > 0 ? 1 : 0) +
  (f.colors.length > 0 ? 1 : 0) +
  (f.year ? 1 : 0) +
  (f.rank ? 1 : 0) +
  (f.thisYear ? 1 : 0) +
  (activeStatus.value ? 1 : 0)
)

const hasActiveFilters = computed(() =>
  !!f.name || !!f.domain || !!f.appellation || f.cepages.length > 0 || f.colors.length > 0 ||
  !!f.year || !!f.rank || f.thisYear || !!activeStatus.value || !!historySearch.value.trim()
)

function clearFilters() {
  Object.assign(f, { name:'', domain:'', appellation:'', cepages:[], colors:[], year:'', rank:'', thisYear: false })
  activeStatus.value = ''; historySearch.value = ''
}

const filteredItems = computed(() => {
  let items = [...cellarStore.items]
  if (f.name.trim())        items = items.filter(i => i.wine.name.toLowerCase().includes(f.name.toLowerCase()))
  if (f.domain.trim())      items = items.filter(i => i.wine.domain.toLowerCase().includes(f.domain.toLowerCase()))
  if (f.appellation.trim()) items = items.filter(i => i.wine.appellation?.toLowerCase().includes(f.appellation.toLowerCase()))
  if (f.cepages.length > 0) {
    const lc = f.cepages.map(c => c.toLowerCase())
    items = items.filter(i => (i.wine.cepages || []).some(c => lc.includes(c.name.toLowerCase())))
  }
  if (f.colors.length > 0)  items = items.filter(i => i.wine.color && f.colors.includes(i.wine.color))
  if (f.year)                items = items.filter(i => String(i.wine.year) === f.year)
  if (f.rank)                items = items.filter(i => String(i.wine.rank) === f.rank)
  if (f.thisYear) {
    items = items.filter(i => {
      const from = i.wine.drinkFromYear, to = i.wine.drinkToYear
      if (!from && !to) return false
      if (from && to) return CURRENT_YEAR >= from && CURRENT_YEAR <= to
      return from ? CURRENT_YEAR >= from : CURRENT_YEAR <= to
    })
  }
  if (activeStatus.value) {
    items = items.filter(i => {
      const s = getDrinkStatus(i.wine)
      return s?.label === STATUS_LABEL[activeStatus.value]
    })
  }
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
      r.wineName.toLowerCase().includes(q) || r.wineDomain.toLowerCase().includes(q) ||
      String(r.wineYear).includes(q) || r.recipeName?.toLowerCase().includes(q) || r.mealNote?.toLowerCase().includes(q)
    )
  }
  return records
})

const stats = computed(() => {
  const totalBottles = cellarStore.items.reduce((sum, i) => sum + i.bottleCount, 0)
  let ready = 0, soon = 0, past = 0
  for (const item of cellarStore.items) {
    const s = getDrinkStatus(item.wine)
    if (!s) continue
    if (s.label === 'Ready')      ready++
    else if (s.label === 'Drink Soon') soon++
    else if (s.label === 'Past Peak')  past++
  }
  return { totalBottles, ready, soon, past }
})

function getStatus(wine) { return getDrinkStatus(wine) }
function formatDate(iso) {
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}
function openDrinkModal(item) { drinkTarget.value = item }
async function confirmDrink(form) {
  await cellarStore.drinkBottle(drinkTarget.value.wineId, form)
  drinkTarget.value = null
}

onMounted(() => { cellarStore.fetchCellar(); cellarStore.fetchHistory() })
</script>
