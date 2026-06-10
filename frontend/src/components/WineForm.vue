<template>
  <form @submit.prevent="handleSubmit" data-testid="wine-form">
    <AlertMessage :message="formError" @dismiss="formError = ''" />

    <!-- Duplicate suggestion -->
    <div v-if="similarWines.length > 0" class="alert alert-warning py-2 mb-3" style="font-size: 0.875rem;">
      <strong>Similar wines already exist:</strong>
      <ul class="mb-0 mt-1">
        <li v-for="w in similarWines" :key="w.id">
          <router-link :to="`/wines/${w.id}`" target="_blank" class="text-dark">
            {{ w.name }} — {{ w.domain }} ({{ w.year }})
          </router-link>
        </li>
      </ul>
      <p class="mb-0 mt-1 text-muted small">Consider adding it to your cellar instead of creating a duplicate.</p>
    </div>

    <div class="row g-3">
      <!-- Core identity -->
      <div class="col-md-6">
        <label class="form-label fw-semibold">Name <span class="text-danger">*</span></label>
        <input
          v-model="form.name"
          type="text"
          class="form-control"
          :class="{ 'is-invalid': errors.name }"
          placeholder="e.g. Château Margaux"
          data-testid="wine-name-input"
          @input="onNameChange"
        />
        <div class="invalid-feedback">{{ errors.name }}</div>
      </div>

      <div class="col-md-6">
        <label class="form-label fw-semibold">Domain / Producer <span class="text-danger">*</span></label>
        <input
          v-model="form.domain"
          type="text"
          class="form-control"
          :class="{ 'is-invalid': errors.domain }"
          placeholder="e.g. Château Margaux"
          data-testid="wine-domain-input"
        />
        <div class="invalid-feedback">{{ errors.domain }}</div>
      </div>

      <div class="col-md-3">
        <label class="form-label fw-semibold">Year <span class="text-danger">*</span></label>
        <input
          v-model.number="form.year"
          type="number"
          class="form-control"
          :class="{ 'is-invalid': errors.year }"
          min="1900"
          :max="new Date().getFullYear() + 2"
          data-testid="wine-year-input"
        />
        <div class="invalid-feedback">{{ errors.year }}</div>
      </div>

      <div class="col-md-3">
        <label class="form-label fw-semibold">Rank</label>
        <select v-model.number="form.rank" class="form-select" data-testid="wine-rank-input">
          <option v-for="n in 5" :key="n" :value="n">{{ '★'.repeat(n) }} ({{ n }})</option>
        </select>
      </div>

      <div class="col-md-3">
        <label class="form-label fw-semibold">Color / Type</label>
        <select v-model="form.color" class="form-select">
          <option value="">— Select —</option>
          <option v-for="c in WINE_COLORS" :key="c" :value="c">{{ c }}</option>
        </select>
      </div>

      <div class="col-md-3">
        <label class="form-label fw-semibold">Country</label>
        <input v-model="form.country" type="text" class="form-control" placeholder="e.g. France" />
      </div>

      <div class="col-md-6">
        <label class="form-label fw-semibold">Region</label>
        <input v-model="form.region" type="text" class="form-control" placeholder="e.g. Bordeaux" />
      </div>

      <div class="col-md-6">
        <label class="form-label fw-semibold">Appellation</label>
        <input v-model="form.appellation" type="text" class="form-control" placeholder="e.g. Margaux" />
      </div>

      <div class="col-md-3">
        <label class="form-label fw-semibold">Drink From</label>
        <input v-model.number="form.drinkFromYear" type="number" class="form-control" min="1900" :max="2100" placeholder="Year" />
      </div>

      <div class="col-md-3">
        <label class="form-label fw-semibold">Drink Until</label>
        <input v-model.number="form.drinkToYear" type="number" class="form-control" min="1900" :max="2100" placeholder="Year" />
      </div>

      <div class="col-12">
        <label class="form-label fw-semibold">Description</label>
        <textarea v-model="form.description" class="form-control" rows="2" placeholder="Tasting notes, profile..." data-testid="wine-description-input"></textarea>
      </div>

      <div class="col-12">
        <label class="form-label fw-semibold">Image URL</label>
        <input v-model="form.imageUrl" type="url" class="form-control" placeholder="https://..." />
      </div>

      <div class="col-12">
        <label class="form-label fw-semibold">Cépages</label>
        <div v-for="(cepage, i) in form.cepages" :key="i" class="row g-2 mb-2 align-items-center">
          <div class="col-7">
            <input
              v-model="cepage.name"
              type="text"
              class="form-control form-control-sm"
              placeholder="Grape variety"
              :data-testid="`cepage-name-${i}`"
            />
          </div>
          <div class="col-3">
            <div class="input-group input-group-sm">
              <input
                v-model.number="cepage.percentage"
                type="number"
                class="form-control"
                min="1"
                max="100"
                :data-testid="`cepage-pct-${i}`"
              />
              <span class="input-group-text">%</span>
            </div>
          </div>
          <div class="col-2">
            <button
              v-if="form.cepages.length > 1"
              type="button"
              class="btn btn-outline-danger btn-sm w-100"
              @click="removeCepage(i)"
            >✕</button>
          </div>
        </div>
        <button type="button" class="btn btn-outline-secondary btn-sm mt-1" @click="addCepage" data-testid="add-cepage-btn">
          + Add Cépage
        </button>
      </div>
    </div>

    <div class="d-flex gap-2 mt-4">
      <button type="submit" class="btn text-white" style="background-color: #4a1020;" :disabled="submitting" data-testid="wine-submit-btn">
        <span v-if="submitting" class="spinner-border spinner-border-sm me-1"></span>
        {{ initialData ? 'Save Changes' : 'Create Wine' }}
      </button>
      <button type="button" class="btn btn-outline-secondary" @click="$emit('cancel')" data-testid="wine-cancel-btn">Cancel</button>
    </div>
  </form>
</template>

<script setup>
import { reactive, ref } from 'vue'
import AlertMessage from './AlertMessage.vue'
import { wineService } from '@/services/wineService'
import { WINE_COLORS } from '@/utils/drinkStatus'

const props = defineProps({
  initialData: { type: Object, default: null }
})

const emit = defineEmits(['submit', 'cancel'])

const form = reactive({
  name: props.initialData?.name ?? '',
  domain: props.initialData?.domain ?? '',
  year: props.initialData?.year ?? new Date().getFullYear(),
  rank: props.initialData?.rank ?? 3,
  color: props.initialData?.color ?? '',
  country: props.initialData?.country ?? '',
  region: props.initialData?.region ?? '',
  appellation: props.initialData?.appellation ?? '',
  description: props.initialData?.description ?? '',
  imageUrl: props.initialData?.imageUrl ?? '',
  drinkFromYear: props.initialData?.drinkFromYear ?? '',
  drinkToYear: props.initialData?.drinkToYear ?? '',
  cepages: props.initialData?.cepages
    ? JSON.parse(JSON.stringify(props.initialData.cepages))
    : [{ name: '', percentage: 100 }]
})

const errors = reactive({})
const formError = ref('')
const submitting = ref(false)
const similarWines = ref([])

let similarTimer = null

function onNameChange() {
  if (props.initialData) return  // don't suggest while editing
  clearTimeout(similarTimer)
  if (!form.name.trim() || form.name.trim().length < 3) {
    similarWines.value = []
    return
  }
  similarTimer = setTimeout(async () => {
    try {
      similarWines.value = await wineService.getSimilar(form.name)
    } catch {
      similarWines.value = []
    }
  }, 400)
}

function validate() {
  Object.keys(errors).forEach(k => delete errors[k])
  if (!form.name.trim()) errors.name = 'Name is required'
  if (!form.domain.trim()) errors.domain = 'Domain is required'
  if (!form.year || form.year < 1900 || form.year > 2100) errors.year = 'Valid year required'
  return Object.keys(errors).length === 0
}

function addCepage() {
  form.cepages.push({ name: '', percentage: 0 })
}

function removeCepage(i) {
  form.cepages.splice(i, 1)
}

async function handleSubmit() {
  if (!validate()) return
  submitting.value = true
  formError.value = ''
  try {
    await emit('submit', JSON.parse(JSON.stringify(form)))
  } catch (err) {
    formError.value = err.message
  } finally {
    submitting.value = false
  }
}
</script>
