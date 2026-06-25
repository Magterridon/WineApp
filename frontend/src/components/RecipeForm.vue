<template>
  <form @submit.prevent="handleSubmit" data-testid="recipe-form" class="space-y-5">
    <AlertMessage :message="formError" @dismiss="formError = ''" />

    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <FormField label="Name" :error="errors.name" required class="sm:col-span-2">
        <input
          v-model="form.name"
          type="text"
          class="input input-bordered w-full"
          :class="errors.name ? 'input-error' : ''"
          placeholder="Recipe name"
          data-testid="recipe-name-input"
        />
      </FormField>

      <FormField label="Type" required>
        <select v-model="form.recipeType" class="select select-bordered w-full" data-testid="recipe-type-input">
          <option>Starter</option>
          <option>Main</option>
          <option>Dessert</option>
          <option>Other</option>
        </select>
      </FormField>
    </div>

    <FormField label="Description">
      <textarea
        v-model="form.description"
        class="textarea textarea-bordered w-full"
        rows="2"
        placeholder="Brief description of the dish"
        data-testid="recipe-description-input"
      ></textarea>
    </FormField>

    <FormField label="Image">
      <ImageUpload v-model="form.imageUrl" />
    </FormField>

    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
      <FormField label="Ingredients" :error="errors.ingredients" hint="One ingredient per line" required>
        <textarea
          v-model="form.ingredientsText"
          class="textarea textarea-bordered w-full"
          :class="errors.ingredients ? 'textarea-error' : ''"
          rows="6"
          placeholder="One ingredient per line&#10;e.g. 500g beef&#10;2 carrots"
          data-testid="recipe-ingredients-input"
        ></textarea>
      </FormField>

      <FormField label="Instructions" :error="errors.instructions" required>
        <textarea
          v-model="form.instructions"
          class="textarea textarea-bordered w-full"
          :class="errors.instructions ? 'textarea-error' : ''"
          rows="6"
          placeholder="Step by step instructions..."
          data-testid="recipe-instructions-input"
        ></textarea>
      </FormField>
    </div>

    <!-- Wine pairings -->
    <div class="space-y-2">
      <label class="block text-sm font-semibold text-base-content/70">Wine Pairings</label>
      <div class="flex flex-wrap gap-2">
        <select v-model="selectedWineId" class="select select-bordered select-sm flex-1 min-w-0" data-testid="pairing-wine-select">
          <option value="">Select a wine…</option>
          <option v-for="w in availableWines" :key="w.id" :value="w.id">
            {{ w.name }} {{ w.year }} — {{ w.domain }}
          </option>
        </select>
        <input
          v-model="selectedWineNotes"
          type="text"
          class="input input-bordered input-sm flex-1 min-w-0"
          placeholder="Pairing notes (optional)"
          data-testid="pairing-notes-input"
        />
        <button
          type="button"
          class="btn btn-sm btn-ghost border border-base-300"
          :disabled="!selectedWineId"
          @click="addPairing"
          data-testid="add-pairing-btn"
        >Add</button>
      </div>

      <div v-if="form.pairings.length" class="rounded-xl border border-base-200 divide-y divide-base-200 overflow-hidden">
        <div v-for="p in form.pairings" :key="p.wineId" class="flex items-center justify-between px-3 py-2 bg-base-100">
          <div class="text-sm">
            <span class="font-medium">{{ wineName(p.wineId) }}</span>
            <span v-if="p.notes" class="text-base-content/50 ml-2">— {{ p.notes }}</span>
          </div>
          <button type="button" class="btn btn-xs btn-ghost text-error" @click="removePairing(p.wineId)">✕</button>
        </div>
      </div>
      <p v-else class="text-sm text-base-content/40">No pairings added yet.</p>
    </div>

    <div class="flex gap-3 pt-2">
      <button type="submit" class="btn btn-primary" :disabled="submitting" data-testid="recipe-submit-btn">
        <span v-if="submitting" class="loading loading-spinner loading-xs"></span>
        {{ initialData ? 'Save Changes' : 'Create Meal' }}
      </button>
      <button type="button" class="btn btn-ghost" @click="$emit('cancel')" data-testid="recipe-cancel-btn">Cancel</button>
    </div>
  </form>
</template>

<script setup>
import { reactive, ref, computed } from 'vue'
import AlertMessage from './AlertMessage.vue'
import ImageUpload  from './ImageUpload.vue'
import FormField    from './ui/FormField.vue'
import { wineService } from '@/services/wineService'

const props = defineProps({ initialData: { type: Object, default: null } })
const emit  = defineEmits(['submit', 'cancel'])

const allWines = ref([])
wineService.getAll().then(w => { allWines.value = w })

const form = reactive({
  name:            props.initialData?.name            ?? '',
  recipeType:      props.initialData?.recipeType      ?? 'Main',
  description:     props.initialData?.description     ?? '',
  imageUrl:        props.initialData?.imageUrl        ?? '',
  ingredientsText: props.initialData?.ingredients?.join('\n') ?? '',
  instructions:    props.initialData?.instructions    ?? '',
  pairings:        props.initialData?.pairings
    ? props.initialData.pairings.map(p => ({ wineId: p.wineId, notes: p.notes || '' }))
    : [],
})

const errors            = reactive({})
const formError         = ref('')
const submitting        = ref(false)
const selectedWineId    = ref('')
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
  selectedWineId.value = selectedWineNotes.value = ''
}

function removePairing(wineId) {
  form.pairings = form.pairings.filter(p => p.wineId !== wineId)
}

function validate() {
  Object.keys(errors).forEach(k => delete errors[k])
  if (!form.name.trim())            errors.name         = 'Name is required'
  if (!form.ingredientsText.trim()) errors.ingredients  = 'Ingredients are required'
  if (!form.instructions.trim())    errors.instructions = 'Instructions are required'
  return Object.keys(errors).length === 0
}

async function handleSubmit() {
  if (!validate()) return
  submitting.value = true
  formError.value  = ''
  try {
    await emit('submit', {
      name:         form.name,
      recipeType:   form.recipeType,
      description:  form.description,
      imageUrl:     form.imageUrl,
      ingredients:  form.ingredientsText.split('\n').map(s => s.trim()).filter(Boolean),
      instructions: form.instructions,
      pairings:     form.pairings.map(p => ({ wineId: p.wineId, notes: p.notes })),
    })
  } catch (err) {
    formError.value = err.message
  } finally {
    submitting.value = false
  }
}
</script>
