<template>
  <form @submit.prevent="handleSubmit" data-testid="wine-form" class="space-y-5">
    <AlertMessage :message="formError" @dismiss="formError = ''" />

    <!-- Duplicate suggestion -->
    <div v-if="similarWines.length > 0" class="rounded-xl bg-warning/10 border border-warning/30 p-3 text-sm">
      <p class="font-semibold text-warning mb-1">Similar wines already exist:</p>
      <ul class="list-disc list-inside space-y-0.5">
        <li v-for="w in similarWines" :key="w.id">
          <router-link :to="`/wines/${w.id}`" target="_blank" class="text-base-content underline hover:text-primary">
            {{ w.name }} — {{ w.domain }} ({{ w.year }})
          </router-link>
        </li>
      </ul>
      <p class="text-xs text-base-content/50 mt-1">Consider adding it to your cellar instead.</p>
    </div>

    <!-- Core identity -->
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
      <FormField label="Name" :error="errors.name" required>
        <input
          v-model="form.name"
          type="text"
          class="input input-bordered w-full"
          :class="errors.name ? 'input-error' : ''"
          placeholder="e.g. Château Margaux"
          data-testid="wine-name-input"
          @input="onNameChange"
        />
      </FormField>

      <FormField label="Domain / Producer" :error="errors.domain" required>
        <input
          v-model="form.domain"
          type="text"
          class="input input-bordered w-full"
          :class="errors.domain ? 'input-error' : ''"
          placeholder="e.g. Château Margaux"
          data-testid="wine-domain-input"
        />
      </FormField>
    </div>

    <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
      <FormField label="Year" :error="errors.year" required>
        <input
          v-model.number="form.year"
          type="number"
          class="input input-bordered w-full"
          :class="errors.year ? 'input-error' : ''"
          :min="1900"
          :max="new Date().getFullYear() + 2"
          data-testid="wine-year-input"
        />
      </FormField>

      <FormField label="Rank">
        <select v-model.number="form.rank" class="select select-bordered w-full" data-testid="wine-rank-input">
          <option v-for="n in 5" :key="n" :value="n">{{ '★'.repeat(n) }} ({{ n }})</option>
        </select>
      </FormField>

      <FormField label="Color / Type">
        <select v-model="form.color" class="select select-bordered w-full">
          <option value="">— Select —</option>
          <option v-for="c in WINE_COLORS" :key="c" :value="c">{{ c }}</option>
        </select>
      </FormField>

      <FormField label="Country">
        <input v-model="form.country" type="text" class="input input-bordered w-full" placeholder="e.g. France" />
      </FormField>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
      <FormField label="Region">
        <input v-model="form.region" type="text" class="input input-bordered w-full" placeholder="e.g. Bordeaux" />
      </FormField>
      <FormField label="Appellation">
        <input v-model="form.appellation" type="text" class="input input-bordered w-full" placeholder="e.g. Margaux" />
      </FormField>
    </div>

    <div class="grid grid-cols-2 gap-4">
      <FormField label="Drink From">
        <input v-model.number="form.drinkFromYear" type="number" class="input input-bordered w-full" min="1900" max="2100" placeholder="Year" />
      </FormField>
      <FormField label="Drink Until">
        <input v-model.number="form.drinkToYear" type="number" class="input input-bordered w-full" min="1900" max="2100" placeholder="Year" />
      </FormField>
    </div>

    <FormField label="Description">
      <textarea
        v-model="form.description"
        class="textarea textarea-bordered w-full"
        rows="2"
        placeholder="Tasting notes, profile…"
        data-testid="wine-description-input"
      ></textarea>
    </FormField>

    <FormField label="Image">
      <ImageUpload v-model="form.imageUrl" />
    </FormField>

    <!-- Cépages -->
    <FormField label="Cépages">
      <div class="space-y-2">
        <div v-for="(cepage, i) in form.cepages" :key="i" class="flex gap-2 items-center">
          <input
            v-model="cepage.name"
            type="text"
            class="input input-bordered input-sm flex-1"
            placeholder="Grape variety"
            :data-testid="`cepage-name-${i}`"
          />
          <div class="flex items-center gap-1">
            <input
              v-model.number="cepage.percentage"
              type="number"
              class="input input-bordered input-sm w-16 text-center"
              min="1"
              max="100"
              :data-testid="`cepage-pct-${i}`"
            />
            <span class="text-sm text-base-content/50">%</span>
          </div>
          <button
            v-if="form.cepages.length > 1"
            type="button"
            class="btn btn-sm btn-ghost text-error"
            @click="removeCepage(i)"
          >✕</button>
        </div>
        <button type="button" class="btn btn-sm btn-ghost border border-base-300 self-start" @click="addCepage" data-testid="add-cepage-btn">
          + Add Cépage
        </button>
      </div>
    </FormField>

    <div class="flex gap-3 pt-2">
      <button type="submit" class="btn btn-primary" :disabled="submitting" data-testid="wine-submit-btn">
        <span v-if="submitting" class="loading loading-spinner loading-xs"></span>
        {{ initialData ? 'Save Changes' : 'Create Wine' }}
      </button>
      <button type="button" class="btn btn-ghost" @click="$emit('cancel')" data-testid="wine-cancel-btn">Cancel</button>
    </div>
  </form>
</template>

<script setup>
import { reactive, ref } from 'vue'
import AlertMessage from './AlertMessage.vue'
import ImageUpload  from './ImageUpload.vue'
import FormField    from './ui/FormField.vue'
import { wineService } from '@/services/wineService'
import { WINE_COLORS } from '@/utils/drinkStatus'

const props = defineProps({ initialData: { type: Object, default: null } })
const emit  = defineEmits(['submit', 'cancel'])

const form = reactive({
  name:          props.initialData?.name          ?? '',
  domain:        props.initialData?.domain        ?? '',
  year:          props.initialData?.year          ?? new Date().getFullYear(),
  rank:          props.initialData?.rank          ?? 3,
  color:         props.initialData?.color         ?? '',
  country:       props.initialData?.country       ?? '',
  region:        props.initialData?.region        ?? '',
  appellation:   props.initialData?.appellation   ?? '',
  description:   props.initialData?.description   ?? '',
  imageUrl:      props.initialData?.imageUrl      ?? '',
  drinkFromYear: props.initialData?.drinkFromYear ?? '',
  drinkToYear:   props.initialData?.drinkToYear   ?? '',
  cepages:       props.initialData?.cepages
    ? JSON.parse(JSON.stringify(props.initialData.cepages))
    : [{ name: '', percentage: 100 }],
})

const errors     = reactive({})
const formError  = ref('')
const submitting = ref(false)
const similarWines = ref([])
let similarTimer = null

function onNameChange() {
  if (props.initialData) return
  clearTimeout(similarTimer)
  if (!form.name.trim() || form.name.trim().length < 3) { similarWines.value = []; return }
  similarTimer = setTimeout(async () => {
    try { similarWines.value = await wineService.getSimilar(form.name) }
    catch { similarWines.value = [] }
  }, 400)
}

function validate() {
  Object.keys(errors).forEach(k => delete errors[k])
  if (!form.name.trim())   errors.name   = 'Name is required'
  if (!form.domain.trim()) errors.domain = 'Domain is required'
  if (!form.year || form.year < 1900 || form.year > 2100) errors.year = 'Valid year required'
  return Object.keys(errors).length === 0
}

function addCepage()     { form.cepages.push({ name: '', percentage: 0 }) }
function removeCepage(i) { form.cepages.splice(i, 1) }

async function handleSubmit() {
  if (!validate()) return
  submitting.value = true
  formError.value  = ''
  try   { await emit('submit', JSON.parse(JSON.stringify(form))) }
  catch (err) { formError.value = err.message }
  finally { submitting.value = false }
}
</script>
