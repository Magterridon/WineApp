<template>
  <div class="space-y-4">

    <PageHeader eyebrow="Admin" title="Pairing Rules">
      <button v-if="!editing" class="btn btn-sm btn-primary" @click="startCreate">+ New Rule</button>
    </PageHeader>

    <AlertMessage v-if="alert" :message="alert.message" :type="alert.type" @dismiss="alert = null" />

    <!-- Create / Edit form -->
    <div v-if="editing" class="bg-base-100 rounded-2xl border border-base-200 overflow-hidden">
      <div class="px-5 py-3 border-b border-base-200 bg-base-200/50 font-semibold text-sm">
        {{ form.id ? 'Edit Rule' : 'New Rule' }}
      </div>
      <div class="p-5 space-y-4">

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <FormField label="Rule name" required class="sm:col-span-2">
            <input v-model="form.name" type="text" class="input input-bordered input-sm w-full" placeholder="e.g. Red Bordeaux with Red Meat" />
          </FormField>
          <FormField label="Priority" hint="Higher = applied first">
            <input v-model.number="form.priority" type="number" min="1" max="100" class="input input-bordered input-sm w-full" />
          </FormField>
        </div>

        <div class="flex items-center gap-2">
          <input v-model="form.isActive" type="checkbox" class="checkbox checkbox-sm checkbox-primary" id="isActiveCheck" />
          <label for="isActiveCheck" class="text-sm font-medium cursor-pointer">Active</label>
        </div>

        <FormField label="Description">
          <input v-model="form.description" type="text" class="input input-bordered input-sm w-full" placeholder="Optional notes about this rule" />
        </FormField>

        <!-- Conditions -->
        <div class="space-y-2">
          <label class="label-caps">Wine conditions <span class="text-error">*</span></label>
          <p class="text-xs text-base-content/40">All conditions must match (AND logic).</p>
          <div v-for="(cond, idx) in form.conditions" :key="idx" class="flex flex-wrap gap-2 items-center">
            <select v-model="cond.field" class="select select-bordered select-sm">
              <option v-for="f in FIELDS" :key="f.value" :value="f.value">{{ f.label }}</option>
            </select>
            <select v-model="cond.operator" class="select select-bordered select-sm">
              <option value="equals">equals</option>
              <option value="contains">contains</option>
              <option value="in">one of</option>
            </select>
            <input v-model="cond.value" type="text" class="input input-bordered input-sm flex-1 min-w-[160px]"
                   :placeholder="cond.operator === 'in' ? 'Red, White, Rosé' : 'Value…'" />
            <button type="button" class="btn btn-xs btn-ghost text-error" @click="form.conditions.splice(idx, 1)">✕</button>
          </div>
          <button type="button" class="btn btn-xs btn-ghost border border-base-200 mt-1"
                  @click="form.conditions.push({ field: 'color', operator: 'equals', value: '' })">
            + Add Condition
          </button>
        </div>

        <!-- Target recipes -->
        <div class="space-y-2">
          <label class="label-caps">Target recipes <span class="text-error">*</span></label>
          <div v-if="allRecipes.length === 0" class="text-base-content/40 text-sm">Loading recipes…</div>
          <div v-else class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-1.5">
            <label v-for="recipe in allRecipes" :key="recipe.id" class="flex items-center gap-2 cursor-pointer text-sm py-1">
              <input type="checkbox" class="checkbox checkbox-xs checkbox-primary" :value="recipe.id" v-model="form.recipeIds" />
              <span class="badge-pill bg-base-200 text-base-content/60 mr-1 text-[10px]">{{ recipe.recipeType }}</span>
              {{ recipe.name }}
            </label>
          </div>
        </div>

        <div class="flex gap-2 pt-2 border-t border-base-200">
          <button class="btn btn-sm btn-primary" :disabled="saving" @click="saveRule">
            <span v-if="saving" class="loading loading-spinner loading-xs"></span>
            {{ form.id ? 'Save Changes' : 'Create Rule' }}
          </button>
          <button class="btn btn-sm btn-ghost" @click="cancelEdit">Cancel</button>
        </div>
      </div>
    </div>

    <LoadingSpinner v-if="loading" />

    <EmptyState
      v-else-if="rules.length === 0 && !editing"
      icon="🔗"
      title="No pairing rules yet"
      body="Create your first rule to power the Weekly Menu."
    />

    <div v-else class="space-y-3">
      <div
        v-for="rule in rules"
        :key="rule.id"
        class="bg-base-100 rounded-2xl border border-base-200 p-4"
        :class="!rule.isActive ? 'opacity-50' : ''"
      >
        <div class="flex flex-wrap items-start justify-between gap-3">
          <div class="flex-1 space-y-2">
            <div class="flex flex-wrap items-center gap-2">
              <span class="font-semibold">{{ rule.name }}</span>
              <span class="badge-pill" :class="rule.isActive ? 'bg-success/15 text-success border-success/25' : 'bg-base-200 text-base-content/40 border-base-300'">
                {{ rule.isActive ? 'Active' : 'Disabled' }}
              </span>
              <span class="badge-pill bg-base-200 text-base-content/50">Priority {{ rule.priority }}</span>
            </div>
            <p v-if="rule.description" class="text-sm text-base-content/50">{{ rule.description }}</p>
            <div v-if="rule.conditions.length > 0" class="flex flex-wrap gap-1">
              <span class="text-xs text-base-content/40 mr-1">Conditions:</span>
              <span v-for="(c, idx) in rule.conditions" :key="idx"
                    class="badge-pill font-mono text-[10px] bg-base-200 text-base-content/70">
                {{ c.field }} {{ c.operator === 'in' ? 'in' : c.operator }} {{ c.value }}
              </span>
            </div>
            <div v-if="rule.recipes.length > 0" class="flex flex-wrap gap-1">
              <span class="text-xs text-base-content/40 mr-1">Targets:</span>
              <span v-for="r in rule.recipes" :key="r.id" class="badge-pill bg-base-200 text-base-content/60 text-[10px]">{{ r.name }}</span>
            </div>
          </div>

          <div class="flex gap-2 flex-shrink-0">
            <button class="btn btn-xs btn-ghost border border-base-200" @click="startEdit(rule)">Edit</button>
            <button class="btn btn-xs btn-ghost border"
                    :class="rule.isActive ? 'border-warning text-warning' : 'border-success text-success'"
                    @click="toggleRule(rule)">
              {{ rule.isActive ? 'Disable' : 'Enable' }}
            </button>
            <button class="btn btn-xs btn-ghost text-error" @click="confirmDelete(rule)">Delete</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { pairingRuleService } from '@/services/pairingRuleService'
import { recipeService }      from '@/services/recipeService'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import AlertMessage   from '@/components/AlertMessage.vue'
import PageHeader     from '@/components/ui/PageHeader.vue'
import EmptyState     from '@/components/ui/EmptyState.vue'
import FormField      from '@/components/ui/FormField.vue'

const FIELDS = [
  { value: 'color', label: 'Color' }, { value: 'region', label: 'Region' },
  { value: 'appellation', label: 'Appellation' }, { value: 'country', label: 'Country' },
  { value: 'domain', label: 'Domain' }, { value: 'cepage', label: 'Cépage' },
  { value: 'name', label: 'Name' }, { value: 'rank', label: 'Rank' },
]

const rules      = ref([])
const allRecipes = ref([])
const loading    = ref(true)
const saving     = ref(false)
const editing    = ref(false)
const alert      = ref(null)

const blankForm = () => ({
  id: null, name: '', description: '', isActive: true, priority: 10,
  conditions: [{ field: 'color', operator: 'equals', value: '' }], recipeIds: [],
})
const form = ref(blankForm())

onMounted(async () => {
  try {
    const [rulesData, recipesData] = await Promise.all([pairingRuleService.getAll(), recipeService.getAll()])
    rules.value = rulesData; allRecipes.value = recipesData
  } catch { showAlert('danger', 'Failed to load data.') }
  finally { loading.value = false }
})

function startCreate() { form.value = blankForm(); editing.value = true; window.scrollTo({ top: 0, behavior: 'smooth' }) }
function startEdit(rule) {
  form.value = { id: rule.id, name: rule.name, description: rule.description ?? '', isActive: rule.isActive, priority: rule.priority, conditions: rule.conditions.map(c => ({ ...c })), recipeIds: rule.recipes.map(r => r.id) }
  editing.value = true; window.scrollTo({ top: 0, behavior: 'smooth' })
}
function cancelEdit() { editing.value = false; form.value = blankForm() }

async function saveRule() {
  if (!form.value.name.trim())                                return showAlert('warning', 'Rule name is required.')
  if (form.value.conditions.length === 0)                     return showAlert('warning', 'At least one condition is required.')
  if (form.value.conditions.some(c => !c.value.trim()))       return showAlert('warning', 'All conditions must have a value.')
  if (form.value.recipeIds.length === 0)                      return showAlert('warning', 'Select at least one target recipe.')
  saving.value = true
  try {
    const payload = { name: form.value.name.trim(), description: form.value.description?.trim() || null, isActive: form.value.isActive, priority: form.value.priority, conditions: form.value.conditions, recipeIds: form.value.recipeIds }
    if (form.value.id) {
      const updated = await pairingRuleService.update(form.value.id, payload)
      const idx = rules.value.findIndex(r => r.id === form.value.id)
      if (idx !== -1) rules.value[idx] = updated
      showAlert('success', 'Rule updated.')
    } else {
      rules.value.unshift(await pairingRuleService.create(payload))
      showAlert('success', 'Rule created.')
    }
    editing.value = false; form.value = blankForm()
  } catch { showAlert('danger', 'Failed to save rule.') }
  finally { saving.value = false }
}

async function toggleRule(rule) {
  try { const updated = await pairingRuleService.toggle(rule.id); const idx = rules.value.findIndex(r => r.id === rule.id); if (idx !== -1) rules.value[idx] = updated }
  catch { showAlert('danger', 'Failed to toggle rule.') }
}

async function confirmDelete(rule) {
  if (!confirm(`Delete rule "${rule.name}"?`)) return
  try { await pairingRuleService.remove(rule.id); rules.value = rules.value.filter(r => r.id !== rule.id); showAlert('success', 'Rule deleted.') }
  catch { showAlert('danger', 'Failed to delete.') }
}

function showAlert(type, message) { alert.value = { type, message }; setTimeout(() => { alert.value = null }, 4000) }
</script>
