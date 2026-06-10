<template>
  <form @submit.prevent="handleSubmit" data-testid="recipe-form">
    <AlertMessage :message="formError" @dismiss="formError = ''" />

    <div class="row g-3">
      <div class="col-md-8">
        <label class="form-label fw-semibold">Name <span class="text-danger">*</span></label>
        <input
          v-model="form.name"
          type="text"
          class="form-control"
          :class="{ 'is-invalid': errors.name }"
          placeholder="Recipe name"
          data-testid="recipe-name-input"
        />
        <div class="invalid-feedback">{{ errors.name }}</div>
      </div>

      <div class="col-md-4">
        <label class="form-label fw-semibold">Type <span class="text-danger">*</span></label>
        <select v-model="form.recipeType" class="form-select" data-testid="recipe-type-input">
          <option>Starter</option>
          <option>Main</option>
          <option>Dessert</option>
          <option>Other</option>
        </select>
      </div>

      <div class="col-12">
        <label class="form-label fw-semibold">Description</label>
        <textarea v-model="form.description" class="form-control" rows="2" placeholder="Brief description of the dish" data-testid="recipe-description-input"></textarea>
      </div>

      <div class="col-12">
        <label class="form-label fw-semibold">Image</label>
        <ImageUpload v-model="form.imageUrl" />
      </div>

      <div class="col-md-6">
        <label class="form-label fw-semibold">Ingredients <span class="text-danger">*</span></label>
        <textarea
          v-model="form.ingredientsText"
          class="form-control"
          :class="{ 'is-invalid': errors.ingredients }"
          rows="6"
          placeholder="One ingredient per line&#10;e.g. 500g beef&#10;2 carrots"
          data-testid="recipe-ingredients-input"
        ></textarea>
        <div class="invalid-feedback">{{ errors.ingredients }}</div>
        <div class="form-text">One ingredient per line</div>
      </div>

      <div class="col-md-6">
        <label class="form-label fw-semibold">Instructions <span class="text-danger">*</span></label>
        <textarea
          v-model="form.instructions"
          class="form-control"
          :class="{ 'is-invalid': errors.instructions }"
          rows="6"
          placeholder="Step by step instructions..."
          data-testid="recipe-instructions-input"
        ></textarea>
        <div class="invalid-feedback">{{ errors.instructions }}</div>
      </div>

      <div class="col-12">
        <label class="form-label fw-semibold">Wine Pairings</label>
        <div class="row g-2 mb-2">
          <div class="col-md-5">
            <select v-model="selectedWineId" class="form-select form-select-sm" data-testid="pairing-wine-select">
              <option value="">Select a wine...</option>
              <option v-for="w in availableWines" :key="w.id" :value="w.id">
                {{ w.name }} {{ w.year }} — {{ w.domain }}
              </option>
            </select>
          </div>
          <div class="col-md-5">
            <input
              v-model="selectedWineNotes"
              type="text"
              class="form-control form-control-sm"
              placeholder="Pairing notes (optional)"
              data-testid="pairing-notes-input"
            />
          </div>
          <div class="col-md-2">
            <button
              type="button"
              class="btn btn-outline-secondary btn-sm w-100"
              :disabled="!selectedWineId"
              @click="addPairing"
              data-testid="add-pairing-btn"
            >Add</button>
          </div>
        </div>

        <div v-if="form.pairings.length" class="list-group list-group-flush border rounded">
          <div v-for="p in form.pairings" :key="p.wineId" class="list-group-item d-flex justify-content-between align-items-start py-2">
            <div>
              <span class="fw-semibold small">{{ wineName(p.wineId) }}</span>
              <span v-if="p.notes" class="text-muted small ms-2">— {{ p.notes }}</span>
            </div>
            <button type="button" class="btn btn-sm btn-link text-danger p-0 ms-2" @click="removePairing(p.wineId)">✕</button>
          </div>
        </div>
        <p v-else class="text-muted small mt-1 mb-0">No pairings added yet.</p>
      </div>
    </div>

    <div class="d-flex gap-2 mt-4">
      <button type="submit" class="btn text-white" style="background-color: #4a1020;" :disabled="submitting" data-testid="recipe-submit-btn">
        <span v-if="submitting" class="spinner-border spinner-border-sm me-1"></span>
        {{ initialData ? 'Save Changes' : 'Create Recipe' }}
      </button>
      <button type="button" class="btn btn-outline-secondary" @click="$emit('cancel')" data-testid="recipe-cancel-btn">Cancel</button>
    </div>
  </form>
</template>

<script setup>
import { reactive, ref, computed } from 'vue'
import AlertMessage from './AlertMessage.vue'
import ImageUpload from './ImageUpload.vue'
import { wineService } from '@/services/wineService'

const props = defineProps({
  initialData: { type: Object, default: null }
})

const emit = defineEmits(['submit', 'cancel'])

const allWines = ref([])
wineService.getAll().then(w => { allWines.value = w })

const form = reactive({
  name: props.initialData?.name ?? '',
  recipeType: props.initialData?.recipeType ?? 'Main',
  description: props.initialData?.description ?? '',
  imageUrl: props.initialData?.imageUrl ?? '',
  ingredientsText: props.initialData?.ingredients?.join('\n') ?? '',
  instructions: props.initialData?.instructions ?? '',
  pairings: props.initialData?.pairings
    ? props.initialData.pairings.map(p => ({ wineId: p.wineId, notes: p.notes || '' }))
    : []
})

const errors = reactive({})
const formError = ref('')
const submitting = ref(false)
const selectedWineId = ref('')
const selectedWineNotes = ref('')

const availableWines = computed(() =>
  allWines.value.filter(w => !form.pairings.some(p => p.wineId === w.id))
)

function wineName(id) {
  const w = allWines.value.find(w => w.id === id)
  return w ? `${w.name} ${w.year}` : `Wine #${id}`
}

function addPairing() {
  if (!selectedWineId.value) return
  form.pairings.push({ wineId: selectedWineId.value, notes: selectedWineNotes.value })
  selectedWineId.value = ''
  selectedWineNotes.value = ''
}

function removePairing(wineId) {
  form.pairings = form.pairings.filter(p => p.wineId !== wineId)
}

function validate() {
  Object.keys(errors).forEach(k => delete errors[k])
  if (!form.name.trim()) errors.name = 'Name is required'
  if (!form.ingredientsText.trim()) errors.ingredients = 'Ingredients are required'
  if (!form.instructions.trim()) errors.instructions = 'Instructions are required'
  return Object.keys(errors).length === 0
}

async function handleSubmit() {
  if (!validate()) return
  submitting.value = true
  formError.value = ''
  try {
    const data = {
      name: form.name,
      recipeType: form.recipeType,
      description: form.description,
      imageUrl: form.imageUrl,
      ingredients: form.ingredientsText.split('\n').map(s => s.trim()).filter(Boolean),
      instructions: form.instructions,
      pairings: form.pairings.map(p => ({ wineId: p.wineId, notes: p.notes }))
    }
    await emit('submit', data)
  } catch (err) {
    formError.value = err.message
  } finally {
    submitting.value = false
  }
}
</script>
