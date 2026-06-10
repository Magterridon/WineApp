<template>
  <div class="cepage-select">
    <div class="input-group input-group-sm">
      <input
        ref="inputEl"
        v-model="inputVal"
        type="text"
        class="form-control"
        :placeholder="placeholder"
        @keydown.enter.prevent="addChip"
        @keydown.188.prevent="addChip"
      />
      <button
        class="btn btn-outline-secondary"
        type="button"
        :disabled="!inputVal.trim()"
        @click="addChip"
        title="Add cépage"
      >+</button>
    </div>

    <!-- Selected chips -->
    <div v-if="modelValue.length > 0" class="d-flex flex-wrap gap-1 mt-1">
      <span
        v-for="chip in modelValue"
        :key="chip"
        class="badge bg-secondary d-inline-flex align-items-center gap-1"
        style="font-size: 0.78rem; padding: 4px 8px;"
      >
        {{ chip }}
        <button
          type="button"
          class="btn-close btn-close-white ms-1"
          style="font-size: 0.55rem"
          :aria-label="`Remove ${chip}`"
          @click="removeChip(chip)"
        />
      </span>
    </div>

    <div v-else class="text-muted" style="font-size:0.75rem; margin-top:2px;">
      Type a cépage name and press Enter to add
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  modelValue:  { type: Array, default: () => [] },
  placeholder: { type: String, default: 'e.g. Merlot, Cabernet Sauvignon…' }
})
const emit = defineEmits(['update:modelValue'])

const inputEl  = ref(null)
const inputVal = ref('')

function addChip() {
  const val = inputVal.value.trim().replace(/,$/, '')
  if (!val) return
  if (props.modelValue.map(c => c.toLowerCase()).includes(val.toLowerCase())) {
    inputVal.value = ''
    return
  }
  emit('update:modelValue', [...props.modelValue, val])
  inputVal.value = ''
}

function removeChip(chip) {
  emit('update:modelValue', props.modelValue.filter(c => c !== chip))
}
</script>
