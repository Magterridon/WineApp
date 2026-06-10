<template>
  <div v-if="show" class="modal fade show d-block" tabindex="-1" @click.self="$emit('cancel')" style="background: rgba(0,0,0,0.5);">
    <div class="modal-dialog modal-dialog-centered modal-lg">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">
            🍷 Drink a bottle
            <span class="text-muted fs-6 fw-normal ms-2">{{ wineName }}</span>
          </h5>
          <button type="button" class="btn-close" @click="$emit('cancel')"></button>
        </div>

        <div class="modal-body">
          <AlertMessage :message="error" @dismiss="error = ''" />

          <!-- Previous tastings for this wine -->
          <div v-if="wineHistory.length > 0" class="mb-4">
            <h6 class="text-muted fw-semibold mb-2">Previous tastings for this wine</h6>
            <div class="table-responsive">
              <table class="table table-sm table-bordered mb-0" style="font-size: 0.85rem;">
                <thead class="table-light">
                  <tr>
                    <th>Date</th>
                    <th>Rating</th>
                    <th>Tasting note</th>
                    <th>Meal</th>
                    <th>Pairing ★</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="r in wineHistory" :key="r.id">
                    <td class="text-nowrap">{{ formatDate(r.consumedAt) }}</td>
                    <td class="text-warning text-nowrap">{{ r.rating ? '★'.repeat(r.rating) : '—' }}</td>
                    <td style="max-width: 160px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                      {{ r.tastingNote || '—' }}
                    </td>
                    <td style="max-width: 140px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                      {{ r.recipeName || r.mealNote || '—' }}
                    </td>
                    <td class="text-warning text-nowrap">{{ r.pairingRating ? '★'.repeat(r.pairingRating) : '—' }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
            <hr class="mt-3" />
          </div>

          <form @submit.prevent="handleSubmit">
            <!-- Tasting section -->
            <h6 class="fw-semibold text-uppercase text-muted small mb-3" style="letter-spacing: .05em;">Tasting</h6>

            <div class="row g-3 mb-3">
              <div class="col-md-4">
                <label class="form-label fw-semibold">Date consumed</label>
                <input v-model="form.consumedAt" type="date" class="form-control" />
              </div>
              <div class="col-md-8">
                <label class="form-label fw-semibold">Wine rating</label>
                <div class="d-flex gap-2 mt-1">
                  <button
                    v-for="n in 5"
                    :key="n"
                    type="button"
                    class="btn btn-sm"
                    :class="form.rating >= n ? 'text-white' : 'btn-outline-secondary'"
                    :style="form.rating >= n ? 'background-color: #4a1020;' : ''"
                    @click="form.rating = form.rating === n ? null : n"
                  >{{ n }}★</button>
                  <span v-if="form.rating" class="text-muted small align-self-center ms-1">({{ form.rating }}/5)</span>
                </div>
              </div>
            </div>

            <div class="mb-4">
              <label class="form-label fw-semibold">Tasting note</label>
              <textarea
                v-model="form.tastingNote"
                class="form-control"
                rows="2"
                placeholder="Aromas, flavours, finish..."
              ></textarea>
            </div>

            <hr />

            <!-- Pairing section -->
            <h6 class="fw-semibold text-uppercase text-muted small mb-3" style="letter-spacing: .05em;">Meal pairing <span class="fw-normal text-muted">(optional)</span></h6>

            <div class="mb-3">
              <label class="form-label fw-semibold">Meal</label>
              <select v-model="mealMode" class="form-select mb-2">
                <option value="none">None</option>
                <option value="recipe">From recipe list</option>
                <option value="custom">Custom meal</option>
              </select>

              <div v-if="mealMode === 'recipe'">
                <select v-model="form.recipeId" class="form-select" :disabled="loadingRecipes">
                  <option :value="null">— select a recipe —</option>
                  <option v-for="r in recipes" :key="r.id" :value="r.id">{{ r.name }}</option>
                </select>
              </div>

              <div v-if="mealMode === 'custom'">
                <input
                  v-model="form.mealNote"
                  type="text"
                  class="form-control"
                  placeholder="e.g. Roast lamb, Cheese board..."
                />
              </div>
            </div>

            <div v-if="mealMode !== 'none'" class="row g-3">
              <div class="col-md-6">
                <label class="form-label fw-semibold">Pairing rating</label>
                <div class="d-flex gap-2 mt-1">
                  <button
                    v-for="n in 5"
                    :key="n"
                    type="button"
                    class="btn btn-sm"
                    :class="form.pairingRating >= n ? 'text-white' : 'btn-outline-secondary'"
                    :style="form.pairingRating >= n ? 'background-color: #722f37;' : ''"
                    @click="form.pairingRating = form.pairingRating === n ? null : n"
                  >{{ n }}★</button>
                </div>
              </div>
              <div class="col-md-6">
                <label class="form-label fw-semibold">Pairing note</label>
                <input
                  v-model="form.pairingNote"
                  type="text"
                  class="form-control"
                  placeholder="How well did it match?"
                />
              </div>
            </div>
          </form>
        </div>

        <div class="modal-footer">
          <button type="button" class="btn btn-outline-secondary" @click="$emit('cancel')">Cancel</button>
          <button
            type="button"
            class="btn text-white"
            style="background-color: #4a1020;"
            :disabled="submitting"
            @click="handleSubmit"
          >
            <span v-if="submitting" class="spinner-border spinner-border-sm me-1"></span>
            Drink this bottle
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, watch } from 'vue'
import AlertMessage from './AlertMessage.vue'
import { recipeService } from '@/services/recipeService'
import { drinkService } from '@/services/drinkService'

const props = defineProps({
  show: { type: Boolean, required: true },
  wineId: { type: Number, default: null },
  wineName: { type: String, default: '' }
})

const emit = defineEmits(['confirm', 'cancel'])

const today = new Date().toISOString().slice(0, 10)

const form = reactive({
  consumedAt: today,
  rating: null,
  tastingNote: '',
  recipeId: null,
  mealNote: '',
  pairingRating: null,
  pairingNote: ''
})

const mealMode = ref('none')
const recipes = ref([])
const wineHistory = ref([])
const loadingRecipes = ref(false)
const error = ref('')
const submitting = ref(false)

function resetForm() {
  form.consumedAt = today
  form.rating = null
  form.tastingNote = ''
  form.recipeId = null
  form.mealNote = ''
  form.pairingRating = null
  form.pairingNote = ''
  mealMode.value = 'none'
  error.value = ''
  wineHistory.value = []
}

watch(() => props.show, async (val) => {
  if (!val) return
  resetForm()

  loadingRecipes.value = true
  try {
    const [hist, recs] = await Promise.all([
      props.wineId ? drinkService.getWineHistory(props.wineId) : Promise.resolve([]),
      recipeService.getAll()
    ])
    wineHistory.value = hist
    recipes.value = recs
  } catch {
    // non-critical — modal still usable
  } finally {
    loadingRecipes.value = false
  }
})

// Clear pairing fields when switching mode
watch(mealMode, (mode) => {
  form.recipeId = null
  form.mealNote = ''
  if (mode === 'none') {
    form.pairingRating = null
    form.pairingNote = ''
  }
})

function formatDate(iso) {
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}

async function handleSubmit() {
  submitting.value = true
  error.value = ''
  try {
    const payload = {
      consumedAt: form.consumedAt || null,
      rating: form.rating || null,
      tastingNote: form.tastingNote || null,
      recipeId: mealMode.value === 'recipe' ? form.recipeId : null,
      mealNote: mealMode.value === 'custom' ? form.mealNote || null : null,
      pairingRating: mealMode.value !== 'none' ? form.pairingRating || null : null,
      pairingNote: mealMode.value !== 'none' ? form.pairingNote || null : null
    }
    await emit('confirm', payload)
  } catch (err) {
    error.value = err.message
  } finally {
    submitting.value = false
  }
}
</script>
