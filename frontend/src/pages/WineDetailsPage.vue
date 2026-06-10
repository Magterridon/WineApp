<template>
  <div>
    <LoadingSpinner v-if="loading" />

    <AlertMessage :message="error" @dismiss="error = ''" />

    <template v-if="wine">
      <div v-if="!editing">
        <div class="d-flex justify-content-between align-items-start mb-3">
          <div>
            <nav aria-label="breadcrumb">
              <ol class="breadcrumb mb-1">
                <li class="breadcrumb-item"><router-link to="/wines">Wines</router-link></li>
                <li class="breadcrumb-item active">{{ wine.name }}</li>
              </ol>
            </nav>
            <h2 class="fw-bold mb-0">{{ wine.name }}</h2>
            <p class="text-muted mb-0">
              {{ wine.domain }} · {{ wine.year }}
              <span v-if="wine.color" class="badge ms-1" :style="colorBadgeStyle" style="font-size: 0.7rem; vertical-align: middle;">{{ wine.color }}</span>
            </p>
            <p v-if="wine.country || wine.region || wine.appellation" class="text-muted small mb-0 mt-1">
              <span v-if="wine.country">{{ wine.country }}</span>
              <span v-if="wine.region"> · {{ wine.region }}</span>
              <span v-if="wine.appellation"> · {{ wine.appellation }}</span>
            </p>
          </div>
          <div class="d-flex gap-2 mt-2">
            <button
              v-if="!cellarStore.isInCellar(wine.id)"
              class="btn text-white btn-sm"
              style="background-color: #4a1020;"
              @click="addToCellar"
              data-testid="add-to-cellar-btn"
            >+ Add to Cellar</button>
            <template v-else>
              <button
                class="btn btn-sm text-white"
                style="background-color: #722f37;"
                @click="showDrinkModal = true"
                data-testid="drink-btn"
              >Drink a bottle</button>
              <button
                class="btn btn-outline-danger btn-sm"
                @click="removeFromCellar"
              >Remove from Cellar</button>
            </template>
            <template v-if="authStore.isAdmin">
              <button class="btn btn-outline-secondary btn-sm" @click="editing = true">Edit</button>
              <button class="btn btn-outline-danger btn-sm" @click="confirmDelete">Delete</button>
            </template>
          </div>
        </div>

        <div class="row g-4">
          <div class="col-md-4">
            <img
              :src="wine.imageUrl || 'https://placehold.co/400x300/4a1020/white?text=Wine'"
              class="img-fluid rounded shadow-sm"
              :alt="wine.name"
            />
          </div>

          <div class="col-md-8">
            <div class="row g-3">
              <div class="col-sm-4">
                <p class="text-muted small mb-1">Rank</p>
                <p class="fs-5 text-warning mb-0">{{ stars(wine.rank) }}</p>
              </div>
              <div class="col-sm-4" v-if="wine.drinkFromYear || wine.drinkToYear">
                <p class="text-muted small mb-1">Drink Window</p>
                <p class="mb-0">{{ wine.drinkFromYear || '?' }} – {{ wine.drinkToYear || '?' }}</p>
              </div>
              <div class="col-sm-4" v-if="drinkStatus">
                <p class="text-muted small mb-1">Status</p>
                <span class="badge fs-6" :class="`bg-${drinkStatus.bg}`">{{ drinkStatus.label }}</span>
              </div>
              <div class="col-12">
                <p class="text-muted small mb-1">Description</p>
                <p class="mb-0">{{ wine.description || '—' }}</p>
              </div>
              <div class="col-12">
                <p class="text-muted small mb-1">Cépages</p>
                <div class="d-flex flex-wrap gap-2">
                  <span
                    v-for="c in wine.cepages"
                    :key="c.name"
                    class="badge text-white"
                    style="background-color: #4a1020;"
                  >{{ c.name }} {{ c.percentage }}%</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-if="linkedRecipes.length" class="mt-4">
          <h5 class="fw-bold mb-3">Recipe Pairings</h5>
          <div class="row g-3">
            <div v-for="r in linkedRecipes" :key="r.id" class="col-md-4">
              <div class="card h-100 shadow-sm">
                <div class="card-body">
                  <span class="badge bg-secondary mb-1">{{ r.recipeType }}</span>
                  <h6 class="card-title">{{ r.name }}</h6>
                  <p class="card-text small text-muted">{{ r.description?.slice(0, 80) }}...</p>
                  <router-link :to="`/recipes/${r.id}`" class="btn btn-outline-secondary btn-sm">View Recipe</router-link>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div v-else-if="authStore.isAdmin">
        <div class="d-flex align-items-center mb-3">
          <button class="btn btn-link p-0 me-2" @click="editing = false">← Back</button>
          <h4 class="fw-bold mb-0">Edit Wine</h4>
        </div>
        <WineForm
          :initial-data="wine"
          @submit="handleUpdate"
          @cancel="editing = false"
        />
      </div>
    </template>

    <DrinkModal
      :show="showDrinkModal"
      :wine-id="wine?.id"
      :wine-name="wine ? `${wine.name} ${wine.year}` : ''"
      @confirm="confirmDrink"
      @cancel="showDrinkModal = false"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { wineService } from '@/services/wineService'
import { recipeService } from '@/services/recipeService'
import { useCellarStore } from '@/stores/cellar'
import { useWinesStore } from '@/stores/wines'
import { useAuthStore } from '@/stores/auth'
import { getDrinkStatus, COLOR_STYLES } from '@/utils/drinkStatus'
import WineForm from '@/components/WineForm.vue'
import DrinkModal from '@/components/DrinkModal.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import AlertMessage from '@/components/AlertMessage.vue'

const route = useRoute()
const router = useRouter()
const cellarStore = useCellarStore()
const winesStore = useWinesStore()
const authStore = useAuthStore()

const wine = ref(null)
const linkedRecipes = ref([])
const loading = ref(true)
const error = ref('')
const editing = ref(false)
const showDrinkModal = ref(false)

const drinkStatus = computed(() => wine.value ? getDrinkStatus(wine.value) : null)
const colorBadgeStyle = computed(() => {
  const s = wine.value?.color ? COLOR_STYLES[wine.value.color] : null
  return s ? `background-color: ${s.bg}; color: ${s.text};` : 'background-color: #6c757d; color: white;'
})

function stars(rank) {
  return '★'.repeat(rank) + '☆'.repeat(5 - rank)
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    const id = Number(route.params.id)
    wine.value = await wineService.getById(id)
    const allRecipes = await recipeService.getAll()
    linkedRecipes.value = allRecipes.filter(r => r.pairings.some(p => p.wineId === id))
    await cellarStore.fetchCellar()
  } catch (err) {
    error.value = err.message
  } finally {
    loading.value = false
  }
}

async function handleUpdate(data) {
  try {
    wine.value = await winesStore.updateWine(wine.value.id, data)
    editing.value = false
  } catch (err) {
    error.value = err.message
    throw err
  }
}

async function confirmDelete() {
  if (!confirm(`Delete "${wine.value.name}"? This cannot be undone.`)) return
  try {
    await winesStore.deleteWine(wine.value.id)
    router.push('/wines')
  } catch (err) {
    error.value = err.message
  }
}

async function addToCellar() {
  await cellarStore.addWine(wine.value.id)
}

async function removeFromCellar() {
  await cellarStore.removeWine(wine.value.id)
}

async function confirmDrink(form) {
  await cellarStore.drinkBottle(wine.value.id, form)
  showDrinkModal.value = false
}

onMounted(load)
</script>
