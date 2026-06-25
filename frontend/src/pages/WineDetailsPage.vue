<template>
  <div>
    <LoadingSpinner v-if="loading" />
    <AlertMessage :message="error" @dismiss="error = ''" />

    <template v-if="wine">
      <!-- View mode -->
      <div v-if="!editing" class="space-y-8">

        <!-- Breadcrumb + actions -->
        <div class="flex flex-wrap items-start justify-between gap-4">
          <div>
            <nav class="text-xs text-base-content/40 mb-2 flex items-center gap-1.5">
              <router-link to="/wines" class="hover:text-primary transition-colors">Wines</router-link>
              <span>›</span>
              <span class="text-base-content/60">{{ wine.name }}</span>
            </nav>
            <h1 class="font-heading text-2xl sm:text-3xl font-bold text-base-content leading-tight">{{ wine.name }}</h1>
            <p class="text-base-content/50 mt-1 flex items-center gap-2 flex-wrap">
              <span>{{ wine.domain }} · {{ wine.year }}</span>
              <WineColorBadge :color="wine.color" />
            </p>
            <p v-if="wine.country || wine.region || wine.appellation" class="text-xs text-base-content/40 mt-1">
              {{ [wine.country, wine.region, wine.appellation].filter(Boolean).join(' · ') }}
            </p>
          </div>

          <div class="flex flex-wrap items-center gap-2 mt-1">
            <button
              v-if="!cellarStore.isInCellar(wine.id)"
              class="btn btn-primary btn-sm"
              @click="addToCellar"
              data-testid="add-to-cellar-btn"
            >+ Add to Cellar</button>
            <template v-else>
              <button class="btn btn-secondary btn-sm" @click="showDrinkModal = true" data-testid="drink-btn">Record tasting</button>
              <button class="btn btn-ghost btn-sm text-error/70 border border-error/15 hover:border-error/40" @click="removeFromCellar">Remove</button>
            </template>
            <template v-if="authStore.isAdmin">
              <div class="w-px h-5 bg-base-300 mx-0.5"></div>
              <button class="btn btn-ghost btn-sm border border-base-200" @click="editing = true">Edit</button>
              <button class="btn btn-ghost btn-sm text-error" @click="confirmDelete">Delete</button>
            </template>
          </div>
        </div>

        <!-- Decorative rule -->
        <div class="wine-rule"></div>

        <!-- Detail layout -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-8">

          <!-- Image column -->
          <div>
            <div class="rounded-2xl overflow-hidden shadow-sm bg-base-200 h-60 md:h-auto md:aspect-[2/3]">
              <img
                :src="wine.imageUrl || 'https://placehold.co/400x600/4a1020/faf8f5?text=🍷'"
                :alt="wine.name"
                class="w-full h-full object-cover"
              />
            </div>
          </div>

          <!-- Info column -->
          <div class="md:col-span-2 space-y-6">

            <!-- Key stats row -->
            <div class="grid grid-cols-2 sm:grid-cols-3 gap-5">
              <div>
                <p class="label-caps mb-2">Rank</p>
                <StarRating :rank="wine.rank" class="text-xl" />
              </div>
              <div v-if="wine.drinkFromYear || wine.drinkToYear">
                <p class="label-caps mb-2">Drink Window</p>
                <p class="font-semibold text-base-content">{{ wine.drinkFromYear || '?' }} – {{ wine.drinkToYear || '?' }}</p>
              </div>
              <div v-if="drinkStatus">
                <p class="label-caps mb-2">Status</p>
                <DrinkStatusBadge :status="drinkStatus" />
              </div>
            </div>

            <!-- Description -->
            <div>
              <p class="label-caps mb-2">Description</p>
              <p class="text-base-content/65 leading-relaxed">{{ wine.description || '—' }}</p>
            </div>

            <!-- Cépages -->
            <div v-if="wine.cepages?.length">
              <p class="label-caps mb-3">Cépages</p>
              <div class="flex flex-wrap gap-2">
                <span
                  v-for="c in wine.cepages"
                  :key="c.name"
                  class="badge-pill bg-primary/10 text-primary border border-primary/20"
                >{{ c.name }}{{ c.percentage ? ` ${c.percentage}%` : '' }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Linked recipes -->
        <div v-if="linkedRecipes.length" class="space-y-4">
          <div class="wine-rule"></div>
          <h2 class="section-title">Meal Pairings</h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
            <div
              v-for="r in linkedRecipes"
              :key="r.id"
              class="bg-base-100 rounded-2xl border border-base-200 p-4 shadow-sm hover:shadow-md transition-shadow"
            >
              <span class="badge-pill bg-base-200 text-base-content/60 mb-3 inline-block">{{ r.recipeType }}</span>
              <h3 class="font-heading font-semibold text-sm leading-snug mb-1">{{ r.name }}</h3>
              <p class="text-xs text-base-content/45 leading-relaxed line-clamp-2 mb-3">{{ r.description }}</p>
              <router-link :to="`/recipes/${r.id}`" class="text-xs font-semibold text-primary border-b border-primary/25 hover:border-primary transition-colors pb-px">
                View Meal →
              </router-link>
            </div>
          </div>
        </div>
      </div>

      <!-- Edit mode -->
      <div v-else-if="authStore.isAdmin" class="space-y-5">
        <div class="flex items-center gap-3">
          <button class="btn btn-ghost btn-sm" @click="editing = false">← Back</button>
          <h2 class="section-title">Edit Wine</h2>
        </div>
        <WineForm :initial-data="wine" @submit="handleUpdate" @cancel="editing = false" />
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
import { wineService }         from '@/services/wineService'
import { recipeService }       from '@/services/recipeService'
import { pairingRuleService }  from '@/services/pairingRuleService'
import { useCellarStore }      from '@/stores/cellar'
import { useWinesStore }       from '@/stores/wines'
import { useAuthStore }        from '@/stores/auth'
import { getDrinkStatus } from '@/utils/drinkStatus'
import WineForm         from '@/components/WineForm.vue'
import DrinkModal       from '@/components/DrinkModal.vue'
import LoadingSpinner   from '@/components/LoadingSpinner.vue'
import AlertMessage     from '@/components/AlertMessage.vue'
import WineColorBadge   from '@/components/ui/WineColorBadge.vue'
import DrinkStatusBadge from '@/components/ui/DrinkStatusBadge.vue'
import StarRating       from '@/components/ui/StarRating.vue'

const route       = useRoute()
const router      = useRouter()
const cellarStore = useCellarStore()
const winesStore  = useWinesStore()
const authStore   = useAuthStore()

const wine           = ref(null)
const linkedRecipes  = ref([])
const loading        = ref(true)
const error          = ref('')
const editing        = ref(false)
const showDrinkModal = ref(false)

const drinkStatus = computed(() => wine.value ? getDrinkStatus(wine.value) : null)


async function load() {
  loading.value = true; error.value = ''
  try {
    const id = Number(route.params.id)
    wine.value = await wineService.getById(id)
    const [allRecipes, ruleCandidates] = await Promise.all([
      recipeService.getAll(),
      pairingRuleService.getCandidates([id]),
    ])
    const ruleRecipeIds = new Set(ruleCandidates.map(c => c.recipeId))
    linkedRecipes.value = allRecipes.filter(r =>
      r.pairings.some(p => p.wineId === id) || ruleRecipeIds.has(r.id)
    )
    await cellarStore.fetchCellar()
  } catch (err) { error.value = err.message }
  finally { loading.value = false }
}

async function handleUpdate(data) {
  try { wine.value = await winesStore.updateWine(wine.value.id, data); editing.value = false }
  catch (err) { error.value = err.message; throw err }
}

async function confirmDelete() {
  if (!confirm(`Delete "${wine.value.name}"? This cannot be undone.`)) return
  try { await winesStore.deleteWine(wine.value.id); router.push('/wines') }
  catch (err) { error.value = err.message }
}

async function addToCellar()    { await cellarStore.addWine(wine.value.id) }
async function removeFromCellar(){ await cellarStore.removeWine(wine.value.id) }
async function confirmDrink(form){ await cellarStore.drinkBottle(wine.value.id, form); showDrinkModal.value = false }

onMounted(load)
</script>
