<template>
  <div class="space-y-4">

    <PageHeader eyebrow="Admin" title="Wine Catalog" />

    <AlertMessage v-if="alert.message" :message="alert.message" :type="alert.type" @dismiss="alert.message = ''" />

    <!-- Filters -->
    <div class="filter-panel">
      <input v-model="filters.search" type="text" class="input input-bordered input-sm w-full" placeholder="Quick search: name, domain, appellation, cépage…" @keyup.enter="applyFilters" />

      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <input v-model="filters.name" type="text" class="input input-bordered input-sm w-full" placeholder="Name…" @keyup.enter="applyFilters" />
        <input v-model="filters.domain" type="text" class="input input-bordered input-sm w-full" placeholder="Domain…" @keyup.enter="applyFilters" />
        <input v-model="filters.appellation" type="text" class="input input-bordered input-sm w-full" placeholder="Appellation…" @keyup.enter="applyFilters" />
      </div>

      <div>
        <label class="label-caps mb-1.5">Cépage</label>
        <CepageSelect v-model="filters.cepages" />
      </div>

      <div>
        <label class="label-caps mb-1.5">Color</label>
        <WineColorPicker v-model="filters.colors" />
      </div>

      <div class="flex flex-wrap gap-2 items-end">
        <div>
          <label class="label-caps mb-1">Vintage</label>
          <input v-model="filters.year" type="number" class="input input-bordered input-sm w-24" placeholder="2015" min="1800" max="2100" />
        </div>
        <div>
          <label class="label-caps mb-1">Rank</label>
          <select v-model="filters.rank" class="select select-bordered select-sm">
            <option value="">All</option>
            <option value="5">★★★★★</option>
            <option value="4">★★★★</option>
            <option value="3">★★★</option>
            <option value="2">★★</option>
            <option value="1">★</option>
          </select>
        </div>
        <div>
          <label class="label-caps mb-1">Image</label>
          <select v-model="filters.hasImage" class="select select-bordered select-sm">
            <option value="">All</option>
            <option value="false">Missing ❌</option>
            <option value="true">Has ✅</option>
          </select>
        </div>
        <div>
          <label class="label-caps mb-1">Pairing</label>
          <select v-model="filters.hasPairing" class="select select-bordered select-sm">
            <option value="">All</option>
            <option value="false">Missing ❌</option>
            <option value="true">Has ✅</option>
          </select>
        </div>
        <div class="flex gap-2 ml-auto">
          <button class="btn btn-primary btn-sm" @click="applyFilters">Search</button>
          <button class="btn btn-ghost btn-sm border border-base-200" @click="clearFilters">Clear</button>
        </div>
      </div>

      <p v-if="!loading" class="text-xs text-base-content/40">
        <span v-if="selectedIds.size > 0" class="text-primary font-semibold">{{ selectedIds.size }} selected · </span>
        Showing {{ showingFrom }}–{{ showingTo }} of {{ response.total }} wines
      </p>
    </div>

    <!-- Bulk actions -->
    <div v-if="selectedIds.size > 0" class="bg-base-100 rounded-2xl border-2 border-primary p-4 space-y-4">
      <div class="flex items-center gap-3">
        <span class="font-semibold text-primary">{{ selectedIds.size }} wine(s) selected</span>
        <button class="btn btn-xs btn-ghost ml-auto" @click="clearSelection">Clear selection</button>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">

        <!-- Bulk image -->
        <div class="space-y-2">
          <h3 class="font-semibold text-sm">📸 Bulk image update</h3>
          <div class="tabs tabs-boxed tabs-sm w-fit">
            <button :class="['tab', imageTab === 'url' ? 'tab-active' : '']" @click="imageTab = 'url'">URL</button>
            <button :class="['tab', imageTab === 'upload' ? 'tab-active' : '']" @click="imageTab = 'upload'">Upload</button>
          </div>
          <div v-if="imageTab === 'url'">
            <input v-model="bulkImage.url" type="url" class="input input-bordered input-sm w-full" placeholder="https://example.com/wine.jpg" />
          </div>
          <div v-else class="flex gap-2">
            <input ref="fileInput" type="file" accept="image/jpeg,image/png,image/gif,image/webp" class="file-input file-input-bordered file-input-sm flex-1" @change="onFileSelected" />
            <button class="btn btn-sm btn-ghost border border-base-200 whitespace-nowrap" :disabled="!bulkImage.file || uploading" @click="uploadFile">
              <span v-if="uploading" class="loading loading-spinner loading-xs"></span>
              Upload
            </button>
          </div>
          <div v-if="bulkImage.url" class="flex gap-2 items-center">
            <img :src="bulkImage.url" alt="Preview" class="h-10 w-10 object-cover rounded-lg" @error="$event.target.style.display='none'" />
            <span class="text-xs text-base-content/50 truncate max-w-[200px]">{{ bulkImage.url }}</span>
          </div>
          <button class="btn btn-sm btn-success" :disabled="!bulkImage.url || applyingImage" @click="applyBulkImage">
            <span v-if="applyingImage" class="loading loading-spinner loading-xs"></span>
            Apply to {{ selectedIds.size }} wine(s)
          </button>
        </div>

        <!-- Bulk pairing -->
        <div class="space-y-2">
          <h3 class="font-semibold text-sm">🍽️ Bulk pairing assignment</h3>
          <div class="relative">
            <input v-model="recipeSearch" type="text" class="input input-bordered input-sm w-full" placeholder="Search recipes…" @input="searchRecipes" @focus="showRecipeDropdown = true" />
            <ul v-if="showRecipeDropdown && recipeSearchResults.length" class="absolute z-20 w-full mt-1 bg-base-100 border border-base-200 rounded-xl shadow-lg max-h-44 overflow-y-auto" @mousedown.prevent>
              <li
                v-for="recipe in recipeSearchResults"
                :key="recipe.id"
                class="px-3 py-2 text-sm hover:bg-base-200 cursor-pointer"
                :class="selectedRecipeIds.has(recipe.id) ? 'text-primary font-medium' : ''"
                @click="toggleRecipe(recipe)"
              >{{ recipe.name }} {{ selectedRecipeIds.has(recipe.id) ? '✓' : '' }}</li>
            </ul>
          </div>
          <div v-if="selectedRecipes.length" class="flex flex-wrap gap-1">
            <span v-for="r in selectedRecipes" :key="r.id" class="badge-pill bg-secondary/15 text-secondary border border-secondary/25 flex items-center gap-1">
              {{ r.name }}
              <button type="button" class="text-xs leading-none ml-0.5 hover:text-secondary/60" @click="removeRecipe(r.id)">✕</button>
            </span>
          </div>
          <p v-else class="text-xs text-base-content/40">No recipes selected yet</p>
          <button class="btn btn-sm btn-success" :disabled="!selectedRecipes.length || applyingPairings" @click="applyBulkPairings">
            <span v-if="applyingPairings" class="loading loading-spinner loading-xs"></span>
            Assign {{ selectedRecipes.length }} recipe(s) to {{ selectedIds.size }} wine(s)
          </button>
        </div>
      </div>
    </div>

    <!-- Wine table -->
    <div class="rounded-2xl border border-base-200 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="table table-sm">
          <thead class="bg-base-200/60 text-base-content/60">
            <tr>
              <th class="w-8">
                <input type="checkbox" class="checkbox checkbox-sm checkbox-primary" :checked="allOnPageSelected" :indeterminate.prop="someOnPageSelected && !allOnPageSelected" @change="toggleSelectPage" />
              </th>
              <th class="w-8">Img</th>
              <th class="w-8">Pair</th>
              <th class="cursor-pointer select-none" @click="setSort('name')">Name {{ sortIcon('name') }}</th>
              <th class="cursor-pointer select-none hidden sm:table-cell" @click="setSort('domain')">Domain {{ sortIcon('domain') }}</th>
              <th class="cursor-pointer select-none" @click="setSort('year')">Year {{ sortIcon('year') }}</th>
              <th class="cursor-pointer select-none hidden md:table-cell" @click="setSort('rank')">Rank {{ sortIcon('rank') }}</th>
              <th class="hidden md:table-cell">Color</th>
              <th class="hidden lg:table-cell cursor-pointer select-none" @click="setSort('appellation')">Appellation {{ sortIcon('appellation') }}</th>
              <th class="w-16"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="10" class="text-center py-8">
                <span class="loading loading-spinner loading-sm text-primary"></span>
              </td>
            </tr>
            <tr v-else-if="response.items.length === 0">
              <td colspan="10" class="text-center py-8 text-base-content/40">No wines match your filters.</td>
            </tr>
            <tr
              v-else
              v-for="wine in response.items"
              :key="wine.id"
              class="hover"
              :class="selectedIds.has(wine.id) ? 'bg-primary/5' : ''"
            >
              <td><input type="checkbox" class="checkbox checkbox-sm" :checked="selectedIds.has(wine.id)" @change="toggleWine(wine.id)" /></td>
              <td class="text-center">{{ wine.hasImage ? '✅' : '❌' }}</td>
              <td class="text-center">{{ wine.hasPairing ? '✅' : '❌' }}</td>
              <td class="font-medium">
                <router-link :to="`/wines/${wine.id}`" class="hover:text-primary no-underline">{{ wine.name }}</router-link>
              </td>
              <td class="text-base-content/50 text-sm hidden sm:table-cell">{{ wine.domain }}</td>
              <td class="text-sm">{{ wine.year }}</td>
              <td class="text-amber-400 text-xs hidden md:table-cell">{{ '★'.repeat(wine.rank) }}</td>
              <td class="hidden md:table-cell">
                <WineColorBadge v-if="wine.color" :color="wine.color" />
              </td>
              <td class="text-xs text-base-content/50 hidden lg:table-cell">{{ wine.appellation }}</td>
              <td>
                <router-link :to="`/wines/${wine.id}`" class="btn btn-xs btn-ghost border border-base-200">Edit</router-link>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="flex items-center justify-between p-3 border-t border-base-200">
        <button class="btn btn-sm btn-ghost border border-base-200" :disabled="currentPage <= 1" @click="goToPage(currentPage - 1)">← Prev</button>
        <span class="text-sm text-base-content/50">Page {{ currentPage }} of {{ totalPages }}</span>
        <button class="btn btn-sm btn-ghost border border-base-200" :disabled="currentPage >= totalPages" @click="goToPage(currentPage + 1)">Next →</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { adminService }      from '@/services/adminService'
import { recipeService }     from '@/services/recipeService'
import WineColorPicker from '@/components/WineColorPicker.vue'
import CepageSelect    from '@/components/CepageSelect.vue'
import AlertMessage    from '@/components/AlertMessage.vue'
import WineColorBadge  from '@/components/ui/WineColorBadge.vue'
import PageHeader      from '@/components/ui/PageHeader.vue'

const loading          = ref(false)
const uploading        = ref(false)
const applyingImage    = ref(false)
const applyingPairings = ref(false)

const alert = reactive({ message: '', type: 'success' })

const response = reactive({ items: [], total: 0, page: 1, pageSize: 50 })

const filters = reactive({
  search: '', name: '', domain: '', appellation: '',
  colors: [], cepages: [], rank: '', year: '', hasImage: '', hasPairing: '',
})

const sort        = reactive({ by: 'name', dir: 'asc' })
const currentPage = ref(1)
const PAGE_SIZE   = 50

function setSort(col) {
  sort.dir = sort.by === col ? (sort.dir === 'asc' ? 'desc' : 'asc') : 'asc'
  sort.by  = col
  applyFilters()
}
function sortIcon(col) { return sort.by === col ? (sort.dir === 'asc' ? '↑' : '↓') : '↕' }

const selectedIds       = ref(new Set())
const allOnPageSelected  = computed(() => response.items.length > 0 && response.items.every(w => selectedIds.value.has(w.id)))
const someOnPageSelected = computed(() => response.items.some(w => selectedIds.value.has(w.id)))

function toggleWine(id) {
  const s = new Set(selectedIds.value); s.has(id) ? s.delete(id) : s.add(id); selectedIds.value = s
}
function toggleSelectPage() {
  const s = new Set(selectedIds.value)
  if (allOnPageSelected.value) response.items.forEach(w => s.delete(w.id))
  else response.items.forEach(w => s.add(w.id))
  selectedIds.value = s
}
function clearSelection() { selectedIds.value = new Set() }

const totalPages  = computed(() => Math.ceil(response.total / PAGE_SIZE) || 1)
const showingFrom = computed(() => response.total === 0 ? 0 : (currentPage.value - 1) * PAGE_SIZE + 1)
const showingTo   = computed(() => Math.min(currentPage.value * PAGE_SIZE, response.total))

async function loadWines(page = 1) {
  loading.value = true; currentPage.value = page
  try {
    const data = await adminService.getWines({
      search:      filters.search      || undefined,
      name:        filters.name        || undefined,
      domain:      filters.domain      || undefined,
      appellation: filters.appellation || undefined,
      colors:      filters.colors.length ? filters.colors : undefined,
      cepages:     filters.cepages.length ? filters.cepages : undefined,
      rank:        filters.rank        ? Number(filters.rank) : undefined,
      year:        filters.year        ? Number(filters.year) : undefined,
      hasImage:    filters.hasImage   === '' ? undefined : filters.hasImage   === 'true',
      hasPairing:  filters.hasPairing === '' ? undefined : filters.hasPairing === 'true',
      sortBy:   sort.by, sortDir: sort.dir, page, pageSize: PAGE_SIZE,
    })
    response.items = data.items; response.total = data.total
  } catch (err) { showAlert(err.message || 'Failed to load wines', 'danger') }
  finally { loading.value = false }
}

function applyFilters() { clearSelection(); loadWines(1) }
function clearFilters() {
  Object.assign(filters, { search:'', name:'', domain:'', appellation:'', colors:[], cepages:[], rank:'', year:'', hasImage:'', hasPairing:'' })
  sort.by = 'name'; sort.dir = 'asc'; clearSelection(); loadWines(1)
}
function goToPage(page) { loadWines(page); window.scrollTo({ top: 0, behavior: 'smooth' }) }

const imageTab  = ref('url')
const fileInput = ref(null)
const bulkImage = reactive({ url: '', file: null })

function onFileSelected(e) { bulkImage.file = e.target.files[0] || null }

async function uploadFile() {
  if (!bulkImage.file) return
  uploading.value = true
  try { const r = await adminService.uploadImage(bulkImage.file); bulkImage.url = r.imageUrl; showAlert('Image uploaded.', 'info') }
  catch (err) { showAlert(err.message || 'Upload failed', 'danger') }
  finally { uploading.value = false }
}

async function applyBulkImage() {
  if (!bulkImage.url || !selectedIds.value.size) return
  applyingImage.value = true
  try { const r = await adminService.bulkSetImage([...selectedIds.value], bulkImage.url); showAlert(r.message || `Applied to ${r.updated} wine(s)`, 'success'); await loadWines(currentPage.value) }
  catch (err) { showAlert(err.message || 'Failed', 'danger') }
  finally { applyingImage.value = false }
}

const recipeSearch        = ref('')
const recipeSearchResults = ref([])
const showRecipeDropdown  = ref(false)
const selectedRecipes     = ref([])
const selectedRecipeIds   = computed(() => new Set(selectedRecipes.value.map(r => r.id)))

let searchTimeout = null
function searchRecipes() {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(async () => {
    if (!recipeSearch.value.trim()) { recipeSearchResults.value = []; return }
    try { recipeSearchResults.value = (await recipeService.getAll(recipeSearch.value)).slice(0, 10) }
    catch { recipeSearchResults.value = [] }
  }, 250)
}

function toggleRecipe(recipe) {
  if (selectedRecipeIds.value.has(recipe.id)) removeRecipe(recipe.id)
  else selectedRecipes.value = [...selectedRecipes.value, { id: recipe.id, name: recipe.name }]
  showRecipeDropdown.value = false; recipeSearch.value = ''; recipeSearchResults.value = []
}
function removeRecipe(id) { selectedRecipes.value = selectedRecipes.value.filter(r => r.id !== id) }

async function applyBulkPairings() {
  if (!selectedRecipes.value.length || !selectedIds.value.size) return
  applyingPairings.value = true
  try {
    const r = await adminService.bulkAssignPairings([...selectedIds.value], selectedRecipes.value.map(r => r.id))
    showAlert(r.message || `Created ${r.created} pairing(s)`, 'success'); await loadWines(currentPage.value); selectedRecipes.value = []
  } catch (err) { showAlert(err.message || 'Failed', 'danger') }
  finally { applyingPairings.value = false }
}

function showAlert(message, type = 'success') {
  alert.message = message; alert.type = type
  if (type === 'success' || type === 'info') setTimeout(() => { alert.message = '' }, 4000)
}

onMounted(() => loadWines(1))
</script>
