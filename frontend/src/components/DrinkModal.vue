<template>
  <div v-if="show" class="fixed inset-0 z-50 flex items-end sm:items-center justify-center p-4" @click.self="$emit('cancel')">
    <div class="absolute inset-0 bg-black/50 backdrop-blur-sm" @click="$emit('cancel')"></div>

    <div class="relative bg-base-100 rounded-2xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">

      <!-- Header -->
      <div class="flex items-center justify-between p-5 border-b border-base-200">
        <div>
          <h3 class="font-heading font-semibold text-lg">Record a tasting</h3>
          <p class="text-sm text-base-content/50 mt-0.5">{{ wineName }}</p>
        </div>
        <button class="btn btn-sm btn-circle btn-ghost" @click="$emit('cancel')">✕</button>
      </div>

      <!-- Body -->
      <div class="p-5 space-y-6">
        <AlertMessage :message="error" @dismiss="error = ''" />

        <!-- Previous tastings -->
        <div v-if="wineHistory.length > 0">
          <p class="label-caps mb-3">Previous tastings</p>
          <div class="overflow-x-auto rounded-xl border border-base-200">
            <table class="table table-sm text-sm">
              <thead class="bg-base-200/60 text-base-content/60">
                <tr>
                  <th>Date</th>
                  <th>Rating</th>
                  <th>Note</th>
                  <th>Meal</th>
                  <th>Pairing ★</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="r in wineHistory" :key="r.id">
                  <td class="whitespace-nowrap">{{ formatDate(r.consumedAt) }}</td>
                  <td class="text-amber-400 whitespace-nowrap">{{ r.rating ? '★'.repeat(r.rating) : '—' }}</td>
                  <td class="max-w-[140px] truncate">{{ r.tastingNote || '—' }}</td>
                  <td class="max-w-[120px] truncate">{{ r.recipeName || r.mealNote || '—' }}</td>
                  <td class="text-amber-400 whitespace-nowrap">{{ r.pairingRating ? '★'.repeat(r.pairingRating) : '—' }}</td>
                </tr>
              </tbody>
            </table>
          </div>
          <div class="border-t border-base-200 my-4"></div>
        </div>

        <!-- Tasting section -->
        <div class="space-y-4">
          <p class="label-caps">Tasting</p>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField label="Date consumed">
              <input v-model="form.consumedAt" type="date" class="input input-bordered input-sm w-full" />
            </FormField>

            <FormField label="Wine rating">
              <div class="flex gap-1.5 flex-wrap">
                <button
                  v-for="n in 5"
                  :key="n"
                  type="button"
                  class="btn btn-sm"
                  :class="form.rating >= n ? 'btn-primary' : 'btn-ghost border border-base-300'"
                  @click="form.rating = form.rating === n ? null : n"
                >{{ n }}★</button>
              </div>
            </FormField>
          </div>

          <FormField label="Tasting note">
            <textarea v-model="form.tastingNote" class="textarea textarea-bordered w-full" rows="2" placeholder="Aromas, flavours, finish…"></textarea>
          </FormField>
        </div>

        <div class="border-t border-base-200"></div>

        <!-- Pairing section -->
        <div class="space-y-4">
          <p class="label-caps">
            Meal pairing
            <span class="normal-case font-normal text-base-content/40 ml-1">(optional)</span>
          </p>

          <FormField label="Meal">
            <select v-model="mealMode" class="select select-bordered select-sm w-full">
              <option value="none">None</option>
              <option value="recipe">From recipe list</option>
              <option value="custom">Custom meal</option>
            </select>
          </FormField>

          <FormField v-if="mealMode === 'recipe'" label="Recipe">
            <select v-model="form.recipeId" class="select select-bordered select-sm w-full" :disabled="loadingRecipes">
              <option :value="null">— select a recipe —</option>
              <option v-for="r in recipes" :key="r.id" :value="r.id">{{ r.name }}</option>
            </select>
          </FormField>

          <FormField v-if="mealMode === 'custom'" label="Meal name">
            <input v-model="form.mealNote" type="text" class="input input-bordered input-sm w-full" placeholder="e.g. Roast lamb, Cheese board…" />
          </FormField>

          <div v-if="mealMode !== 'none'" class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField label="Pairing rating">
              <div class="flex gap-1.5 flex-wrap">
                <button
                  v-for="n in 5"
                  :key="n"
                  type="button"
                  class="btn btn-xs"
                  :class="form.pairingRating >= n ? 'btn-secondary' : 'btn-ghost border border-base-300'"
                  @click="form.pairingRating = form.pairingRating === n ? null : n"
                >{{ n }}★</button>
              </div>
            </FormField>

            <FormField label="Pairing note">
              <input v-model="form.pairingNote" type="text" class="input input-bordered input-sm w-full" placeholder="How well did it match?" />
            </FormField>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="flex justify-end gap-3 p-5 border-t border-base-200">
        <button class="btn btn-ghost" @click="$emit('cancel')">Cancel</button>
        <button class="btn btn-primary" :disabled="submitting" @click="handleSubmit">
          <span v-if="submitting" class="loading loading-spinner loading-xs"></span>
          Drink this bottle
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, watch } from 'vue'
import AlertMessage from './AlertMessage.vue'
import FormField    from './ui/FormField.vue'
import { recipeService } from '@/services/recipeService'
import { drinkService }  from '@/services/drinkService'

const props = defineProps({
  show:     { type: Boolean, required: true },
  wineId:   { type: Number,  default: null },
  wineName: { type: String,  default: '' },
})
const emit = defineEmits(['confirm', 'cancel'])

const today = new Date().toISOString().slice(0, 10)

const form = reactive({
  consumedAt: today, rating: null, tastingNote: '',
  recipeId: null, mealNote: '', pairingRating: null, pairingNote: '',
})

const mealMode       = ref('none')
const recipes        = ref([])
const wineHistory    = ref([])
const loadingRecipes = ref(false)
const error          = ref('')
const submitting     = ref(false)

function resetForm() {
  form.consumedAt = today; form.rating = null; form.tastingNote = ''
  form.recipeId = null; form.mealNote = ''; form.pairingRating = null; form.pairingNote = ''
  mealMode.value = 'none'; error.value = ''; wineHistory.value = []
}

watch(() => props.show, async (val) => {
  if (!val) return
  resetForm()
  loadingRecipes.value = true
  try {
    const [hist, recs] = await Promise.all([
      props.wineId ? drinkService.getWineHistory(props.wineId) : Promise.resolve([]),
      recipeService.getAll(),
    ])
    wineHistory.value = hist
    recipes.value = recs
  } catch { /* non-critical */ }
  finally { loadingRecipes.value = false }
})

watch(mealMode, () => {
  form.recipeId = null; form.mealNote = ''
  if (mealMode.value === 'none') { form.pairingRating = null; form.pairingNote = '' }
})

function formatDate(iso) {
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}

async function handleSubmit() {
  submitting.value = true; error.value = ''
  try {
    await emit('confirm', {
      consumedAt:    form.consumedAt || null,
      rating:        form.rating || null,
      tastingNote:   form.tastingNote || null,
      recipeId:      mealMode.value === 'recipe' ? form.recipeId : null,
      mealNote:      mealMode.value === 'custom'  ? form.mealNote || null : null,
      pairingRating: mealMode.value !== 'none'    ? form.pairingRating || null : null,
      pairingNote:   mealMode.value !== 'none'    ? form.pairingNote  || null : null,
    })
  } catch (err) {
    error.value = err.message
  } finally {
    submitting.value = false
  }
}
</script>
