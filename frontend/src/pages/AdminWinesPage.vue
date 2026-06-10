<template>
  <div class="container-fluid py-4">

    <!-- Page header -->
    <div class="d-flex align-items-center mb-4 gap-3">
      <h1 class="h3 mb-0">🍷 Wine Catalog Admin</h1>
      <span class="badge bg-danger">Admin only</span>
    </div>

    <!-- Alert feedback -->
    <div v-if="alert.message" :class="`alert alert-${alert.type} alert-dismissible`" role="alert">
      {{ alert.message }}
      <button type="button" class="btn-close" @click="alert.message = ''" aria-label="Close"></button>
    </div>

    <!-- ── Filters ─────────────────────────────────────────────────────── -->
    <div class="card mb-3 border-0 bg-light">
      <div class="card-body py-2">
        <div class="row g-2 align-items-end">

          <!-- Text search -->
          <div class="col-12 col-md-3">
            <label class="form-label small mb-1">Search (name / domain / appellation / cepage)</label>
            <input
              v-model="filters.search"
              type="text"
              class="form-control form-control-sm"
              placeholder="e.g. Château Margaux"
              @keyup.enter="applyFilters"
            />
          </div>

          <!-- Appellation -->
          <div class="col-6 col-md-2">
            <label class="form-label small mb-1">Appellation</label>
            <input
              v-model="filters.appellation"
              type="text"
              class="form-control form-control-sm"
              placeholder="e.g. Pauillac"
              @keyup.enter="applyFilters"
            />
          </div>

          <!-- Color -->
          <div class="col-6 col-md-1">
            <label class="form-label small mb-1">Color</label>
            <select v-model="filters.color" class="form-select form-select-sm">
              <option value="">All</option>
              <option>Red</option>
              <option>White</option>
              <option>Rosé</option>
              <option>Sparkling</option>
              <option>Fortified</option>
              <option>Orange</option>
            </select>
          </div>

          <!-- Rank -->
          <div class="col-4 col-md-1">
            <label class="form-label small mb-1">Rank</label>
            <select v-model="filters.rank" class="form-select form-select-sm">
              <option value="">All</option>
              <option value="5">★★★★★</option>
              <option value="4">★★★★</option>
              <option value="3">★★★</option>
              <option value="2">★★</option>
              <option value="1">★</option>
            </select>
          </div>

          <!-- Year -->
          <div class="col-4 col-md-1">
            <label class="form-label small mb-1">Vintage</label>
            <input
              v-model="filters.year"
              type="number"
              class="form-control form-control-sm"
              placeholder="2015"
              min="1800" max="2100"
            />
          </div>

          <!-- Image -->
          <div class="col-4 col-md-1">
            <label class="form-label small mb-1">Image</label>
            <select v-model="filters.hasImage" class="form-select form-select-sm">
              <option value="">All</option>
              <option value="false">Missing ❌</option>
              <option value="true">Has ✅</option>
            </select>
          </div>

          <!-- Pairing -->
          <div class="col-6 col-md-1">
            <label class="form-label small mb-1">Pairing</label>
            <select v-model="filters.hasPairing" class="form-select form-select-sm">
              <option value="">All</option>
              <option value="false">Missing ❌</option>
              <option value="true">Has ✅</option>
            </select>
          </div>

          <!-- Buttons -->
          <div class="col-6 col-md-2 d-flex gap-2 align-items-end">
            <button class="btn btn-primary btn-sm" @click="applyFilters">
              🔍 Search
            </button>
            <button class="btn btn-outline-secondary btn-sm" @click="clearFilters">
              Clear
            </button>
          </div>

        </div>

        <!-- Sort row -->
        <div class="row g-2 mt-1 align-items-center">
          <div class="col-auto">
            <label class="form-label small mb-0 me-1">Sort by:</label>
            <select v-model="sort.by" class="form-select form-select-sm d-inline-block w-auto" @change="applyFilters">
              <option value="name">Name</option>
              <option value="domain">Domain</option>
              <option value="year">Vintage</option>
              <option value="rank">Rank</option>
              <option value="appellation">Appellation</option>
              <option value="region">Region</option>
              <option value="createdAt">Import date</option>
              <option value="hasImage">Image (missing first)</option>
              <option value="hasPairing">Pairing (missing first)</option>
            </select>
            <select v-model="sort.dir" class="form-select form-select-sm d-inline-block w-auto ms-1" @change="applyFilters">
              <option value="asc">Asc ↑</option>
              <option value="desc">Desc ↓</option>
            </select>
          </div>
          <div class="col-auto ms-auto text-muted small">
            <template v-if="!loading">
              <span v-if="selectedIds.size > 0" class="text-primary fw-semibold">
                {{ selectedIds.size }} selected ·
              </span>
              Showing {{ showingFrom }}–{{ showingTo }} of {{ response.total }} wines
            </template>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Bulk actions panel (visible when selection > 0) ──────────────────── -->
    <div v-if="selectedIds.size > 0" class="card border-primary mb-3">
      <div class="card-header bg-primary text-white py-2 d-flex align-items-center gap-3">
        <strong>{{ selectedIds.size }} wine(s) selected</strong>
        <button class="btn btn-sm btn-outline-light ms-auto" @click="clearSelection">
          Clear selection
        </button>
      </div>
      <div class="card-body">
        <div class="row g-4">

          <!-- Image bulk update -->
          <div class="col-12 col-md-6">
            <h6 class="fw-semibold">📸 Bulk image update</h6>

            <!-- Tab: URL or upload -->
            <ul class="nav nav-tabs nav-sm mb-2">
              <li class="nav-item">
                <button :class="['nav-link py-1 px-3 small', imageTab === 'url' ? 'active' : '']" @click="imageTab = 'url'">URL</button>
              </li>
              <li class="nav-item">
                <button :class="['nav-link py-1 px-3 small', imageTab === 'upload' ? 'active' : '']" @click="imageTab = 'upload'">Upload file</button>
              </li>
            </ul>

            <!-- URL input -->
            <div v-if="imageTab === 'url'">
              <input
                v-model="bulkImage.url"
                type="url"
                class="form-control form-control-sm"
                placeholder="https://example.com/wine.jpg"
              />
            </div>

            <!-- File upload -->
            <div v-else class="d-flex gap-2 align-items-center">
              <input
                ref="fileInput"
                type="file"
                accept="image/jpeg,image/png,image/gif,image/webp"
                class="form-control form-control-sm"
                @change="onFileSelected"
              />
              <button
                class="btn btn-sm btn-outline-secondary text-nowrap"
                :disabled="!bulkImage.file || uploading"
                @click="uploadFile"
              >
                <span v-if="uploading" class="spinner-border spinner-border-sm me-1"></span>
                Upload
              </button>
            </div>

            <!-- Preview of current image URL -->
            <div v-if="bulkImage.url" class="mt-2 d-flex gap-2 align-items-center">
              <img
                :src="bulkImage.url"
                alt="Preview"
                style="height:40px;width:40px;object-fit:cover;border-radius:4px;"
                @error="$event.target.style.display='none'"
              />
              <span class="small text-muted text-truncate" style="max-width:220px">{{ bulkImage.url }}</span>
            </div>

            <button
              class="btn btn-sm btn-success mt-2"
              :disabled="!bulkImage.url || applyingImage"
              @click="applyBulkImage"
            >
              <span v-if="applyingImage" class="spinner-border spinner-border-sm me-1"></span>
              Apply to {{ selectedIds.size }} wine(s)
            </button>
          </div>

          <!-- Pairing bulk assign -->
          <div class="col-12 col-md-6">
            <h6 class="fw-semibold">🍽️ Bulk pairing assignment</h6>

            <!-- Recipe search -->
            <div class="position-relative mb-2">
              <input
                v-model="recipeSearch"
                type="text"
                class="form-control form-control-sm"
                placeholder="Search recipes…"
                @input="searchRecipes"
                @focus="showRecipeDropdown = true"
              />
              <ul
                v-if="showRecipeDropdown && recipeSearchResults.length"
                class="list-group position-absolute w-100 shadow-sm"
                style="z-index:200;max-height:180px;overflow-y:auto;"
                @mousedown.prevent
              >
                <li
                  v-for="recipe in recipeSearchResults"
                  :key="recipe.id"
                  class="list-group-item list-group-item-action py-1 small"
                  :class="{ active: selectedRecipeIds.has(recipe.id) }"
                  @click="toggleRecipe(recipe)"
                >
                  {{ recipe.name }}
                  <span v-if="selectedRecipeIds.has(recipe.id)" class="ms-1">✓</span>
                </li>
              </ul>
            </div>

            <!-- Selected recipes chips -->
            <div class="d-flex flex-wrap gap-1 mb-2" v-if="selectedRecipes.length">
              <span
                v-for="r in selectedRecipes"
                :key="r.id"
                class="badge bg-secondary d-flex align-items-center gap-1"
              >
                {{ r.name }}
                <button
                  type="button"
                  class="btn-close btn-close-white"
                  style="font-size:0.6rem"
                  @click="removeRecipe(r.id)"
                  :aria-label="`Remove ${r.name}`"
                ></button>
              </span>
            </div>
            <p v-else class="text-muted small mb-2">No recipes selected yet</p>

            <button
              class="btn btn-sm btn-success"
              :disabled="!selectedRecipes.length || applyingPairings"
              @click="applyBulkPairings"
            >
              <span v-if="applyingPairings" class="spinner-border spinner-border-sm me-1"></span>
              Assign {{ selectedRecipes.length }} recipe(s) to {{ selectedIds.size }} wine(s)
            </button>
          </div>

        </div>
      </div>
    </div>

    <!-- ── Wine table ────────────────────────────────────────────────────────── -->
    <div class="card border-0 shadow-sm">
      <div class="table-responsive">
        <table class="table table-hover table-sm mb-0 align-middle">
          <thead class="table-dark">
            <tr>
              <th style="width:36px">
                <input
                  type="checkbox"
                  class="form-check-input"
                  :checked="allOnPageSelected"
                  :indeterminate.prop="someOnPageSelected && !allOnPageSelected"
                  @change="toggleSelectPage"
                  title="Select all on page"
                />
              </th>
              <th style="width:36px" title="Image">Img</th>
              <th style="width:36px" title="Pairing">Pair</th>
              <th>Name</th>
              <th>Domain</th>
              <th>Vintage</th>
              <th>Rank</th>
              <th>Color</th>
              <th>Appellation</th>
              <th style="width:80px"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="10" class="text-center py-4">
                <div class="spinner-border spinner-border-sm text-secondary me-2"></div>
                Loading…
              </td>
            </tr>
            <tr v-else-if="response.items.length === 0">
              <td colspan="10" class="text-center py-4 text-muted">No wines match your filters.</td>
            </tr>
            <tr
              v-else
              v-for="wine in response.items"
              :key="wine.id"
              :class="{ 'table-active': selectedIds.has(wine.id) }"
            >
              <td>
                <input
                  type="checkbox"
                  class="form-check-input"
                  :checked="selectedIds.has(wine.id)"
                  @change="toggleWine(wine.id)"
                />
              </td>
              <td>
                <span :title="wine.hasImage ? wine.imageUrl : 'No image'" style="font-size:1.1em">
                  {{ wine.hasImage ? '✅' : '❌' }}
                </span>
              </td>
              <td>
                <span :title="wine.hasPairing ? `${wine.pairingCount} pairing(s)` : 'No pairings'" style="font-size:1.1em">
                  {{ wine.hasPairing ? '✅' : '❌' }}
                </span>
              </td>
              <td class="fw-medium">
                <router-link :to="`/wines/${wine.id}`" class="text-decoration-none text-dark">
                  {{ wine.name }}
                </router-link>
              </td>
              <td class="text-muted small">{{ wine.domain }}</td>
              <td class="small">{{ wine.year }}</td>
              <td class="small">{{ '★'.repeat(wine.rank) }}</td>
              <td class="small">
                <span v-if="wine.color" :class="colorBadgeClass(wine.color)" class="badge">
                  {{ wine.color }}
                </span>
              </td>
              <td class="small text-muted">{{ wine.appellation }}</td>
              <td>
                <router-link
                  :to="`/wines/${wine.id}`"
                  class="btn btn-outline-secondary btn-sm py-0 px-2"
                  title="Edit wine"
                >
                  Edit
                </router-link>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="card-footer d-flex align-items-center justify-content-between py-2">
        <button
          class="btn btn-sm btn-outline-secondary"
          :disabled="currentPage <= 1"
          @click="goToPage(currentPage - 1)"
        >
          ← Prev
        </button>
        <span class="small text-muted">
          Page {{ currentPage }} of {{ totalPages }}
        </span>
        <button
          class="btn btn-sm btn-outline-secondary"
          :disabled="currentPage >= totalPages"
          @click="goToPage(currentPage + 1)"
        >
          Next →
        </button>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { adminService } from '@/services/adminService'
import { recipeService } from '@/services/recipeService'

// ── State ─────────────────────────────────────────────────────────────────────

const loading      = ref(false)
const uploading    = ref(false)
const applyingImage   = ref(false)
const applyingPairings = ref(false)

const alert = reactive({ message: '', type: 'success' })

const response = reactive({
  items: [],
  total: 0,
  page:  1,
  pageSize: 50
})

const filters = reactive({
  search:      '',
  appellation: '',
  color:       '',
  rank:        '',
  year:        '',
  hasImage:    '',
  hasPairing:  ''
})

const sort = reactive({ by: 'name', dir: 'asc' })

const currentPage = ref(1)
const PAGE_SIZE   = 50

// ── Selection ─────────────────────────────────────────────────────────────────

const selectedIds = ref(new Set())

const allOnPageSelected = computed(() =>
  response.items.length > 0 &&
  response.items.every(w => selectedIds.value.has(w.id))
)

const someOnPageSelected = computed(() =>
  response.items.some(w => selectedIds.value.has(w.id))
)

function toggleWine(id) {
  const s = new Set(selectedIds.value)
  s.has(id) ? s.delete(id) : s.add(id)
  selectedIds.value = s
}

function toggleSelectPage() {
  const s = new Set(selectedIds.value)
  if (allOnPageSelected.value) {
    response.items.forEach(w => s.delete(w.id))
  } else {
    response.items.forEach(w => s.add(w.id))
  }
  selectedIds.value = s
}

function clearSelection() {
  selectedIds.value = new Set()
}

// ── Pagination helpers ────────────────────────────────────────────────────────

const totalPages  = computed(() => Math.ceil(response.total / PAGE_SIZE) || 1)
const showingFrom = computed(() => response.total === 0 ? 0 : (currentPage.value - 1) * PAGE_SIZE + 1)
const showingTo   = computed(() => Math.min(currentPage.value * PAGE_SIZE, response.total))

// ── Data loading ──────────────────────────────────────────────────────────────

async function loadWines(page = 1) {
  loading.value = true
  currentPage.value = page
  try {
    const query = {
      search:      filters.search     || undefined,
      appellation: filters.appellation || undefined,
      color:       filters.color       || undefined,
      rank:        filters.rank        ? Number(filters.rank) : undefined,
      year:        filters.year        ? Number(filters.year) : undefined,
      hasImage:    filters.hasImage === '' ? undefined : filters.hasImage === 'true',
      hasPairing:  filters.hasPairing === '' ? undefined : filters.hasPairing === 'true',
      sortBy:      sort.by,
      sortDir:     sort.dir,
      page,
      pageSize:    PAGE_SIZE
    }
    const data = await adminService.getWines(query)
    response.items    = data.items
    response.total    = data.total
    response.page     = data.page
    response.pageSize = data.pageSize
  } catch (err) {
    showAlert(err.message || 'Failed to load wines', 'danger')
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  clearSelection()
  loadWines(1)
}

function clearFilters() {
  Object.assign(filters, { search: '', appellation: '', color: '', rank: '', year: '', hasImage: '', hasPairing: '' })
  sort.by  = 'name'
  sort.dir = 'asc'
  clearSelection()
  loadWines(1)
}

function goToPage(page) {
  loadWines(page)
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

// ── Bulk image ────────────────────────────────────────────────────────────────

const imageTab  = ref('url')
const fileInput = ref(null)
const bulkImage = reactive({ url: '', file: null })

function onFileSelected(event) {
  bulkImage.file = event.target.files[0] || null
}

async function uploadFile() {
  if (!bulkImage.file) return
  uploading.value = true
  try {
    const result  = await adminService.uploadImage(bulkImage.file)
    bulkImage.url = result.imageUrl
    showAlert('Image uploaded. Click "Apply" to assign it to selected wines.', 'info')
  } catch (err) {
    showAlert(err.message || 'Upload failed', 'danger')
  } finally {
    uploading.value = false
  }
}

async function applyBulkImage() {
  if (!bulkImage.url || !selectedIds.value.size) return
  applyingImage.value = true
  try {
    const result = await adminService.bulkSetImage([...selectedIds.value], bulkImage.url)
    showAlert(result.message || `Image applied to ${result.updated} wine(s)`, 'success')
    await loadWines(currentPage.value)
  } catch (err) {
    showAlert(err.message || 'Failed to apply image', 'danger')
  } finally {
    applyingImage.value = false
  }
}

// ── Bulk pairings ─────────────────────────────────────────────────────────────

const recipeSearch        = ref('')
const recipeSearchResults = ref([])
const showRecipeDropdown  = ref(false)
const selectedRecipes     = ref([])
const selectedRecipeIds   = computed(() => new Set(selectedRecipes.value.map(r => r.id)))

let searchTimeout = null
function searchRecipes() {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(async () => {
    if (!recipeSearch.value.trim()) {
      recipeSearchResults.value = []
      return
    }
    try {
      const results = await recipeService.getAll(recipeSearch.value)
      recipeSearchResults.value = results.slice(0, 10)
    } catch {
      recipeSearchResults.value = []
    }
  }, 250)
}

function toggleRecipe(recipe) {
  if (selectedRecipeIds.value.has(recipe.id)) {
    removeRecipe(recipe.id)
  } else {
    selectedRecipes.value = [...selectedRecipes.value, { id: recipe.id, name: recipe.name }]
  }
  showRecipeDropdown.value = false
  recipeSearch.value = ''
  recipeSearchResults.value = []
}

function removeRecipe(id) {
  selectedRecipes.value = selectedRecipes.value.filter(r => r.id !== id)
}

async function applyBulkPairings() {
  if (!selectedRecipes.value.length || !selectedIds.value.size) return
  applyingPairings.value = true
  try {
    const result = await adminService.bulkAssignPairings(
      [...selectedIds.value],
      selectedRecipes.value.map(r => r.id)
    )
    showAlert(result.message || `Created ${result.created} pairing(s)`, 'success')
    await loadWines(currentPage.value)
    selectedRecipes.value = []
  } catch (err) {
    showAlert(err.message || 'Failed to assign pairings', 'danger')
  } finally {
    applyingPairings.value = false
  }
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function showAlert(message, type = 'success') {
  alert.message = message
  alert.type    = type
  if (type === 'success' || type === 'info') {
    setTimeout(() => { alert.message = '' }, 4000)
  }
}

function colorBadgeClass(color) {
  const map = {
    Red: 'bg-danger',
    White: 'bg-warning text-dark',
    Rosé: 'bg-pink text-dark',
    Sparkling: 'bg-info text-dark',
    Fortified: 'bg-secondary',
    Orange: 'bg-orange text-dark'
  }
  return map[color] ?? 'bg-secondary'
}

// Close recipe dropdown on outside click
function handleOutsideClick(e) {
  if (!e.target.closest('[data-recipe-picker]')) {
    showRecipeDropdown.value = false
  }
}

// ── Init ──────────────────────────────────────────────────────────────────────

onMounted(() => {
  loadWines(1)
  document.addEventListener('click', handleOutsideClick)
})
</script>
