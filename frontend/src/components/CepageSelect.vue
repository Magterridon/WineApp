<template>
  <div class="space-y-1.5">
    <div class="flex gap-2">
      <input
        ref="inputEl"
        v-model="inputVal"
        type="text"
        class="input input-bordered input-sm flex-1"
        :placeholder="placeholder"
        @keydown.enter.prevent="addChip"
        @keydown.188.prevent="addChip"
      />
      <button class="btn btn-sm btn-ghost border border-base-300" type="button" :disabled="!inputVal.trim()" @click="addChip">+</button>
    </div>

    <div v-if="modelValue.length > 0" class="flex flex-wrap gap-1">
      <span
        v-for="chip in modelValue"
        :key="chip"
        class="badge badge-sm badge-secondary gap-1"
      >
        {{ chip }}
        <button type="button" class="text-xs leading-none" :aria-label="`Remove ${chip}`" @click="removeChip(chip)">✕</button>
      </span>
    </div>
    <p v-else class="text-xs text-base-content/40">Type a cépage name and press Enter to add</p>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  modelValue:  { type: Array,  default: () => [] },
  placeholder: { type: String, default: 'e.g. Merlot, Cabernet Sauvignon…' },
})
const emit = defineEmits(['update:modelValue'])

const inputEl  = ref(null)
const inputVal = ref('')

function addChip() {
  const val = inputVal.value.trim().replace(/,$/, '')
  if (!val) return
  if (props.modelValue.map(c => c.toLowerCase()).includes(val.toLowerCase())) { inputVal.value = ''; return }
  emit('update:modelValue', [...props.modelValue, val])
  inputVal.value = ''
}

function removeChip(chip) {
  emit('update:modelValue', props.modelValue.filter(c => c !== chip))
}
</script>
