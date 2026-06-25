<template>
  <div class="flex flex-wrap gap-2" role="group" :aria-label="label">
    <button
      v-for="c in WINE_COLORS"
      :key="c"
      type="button"
      class="btn btn-sm gap-1.5 transition-all duration-150 hover:-translate-y-0.5"
      :class="isActive(c) ? 'shadow-sm' : 'btn-ghost border border-base-300'"
      :style="isActive(c) ? `background:${COLOR_STYLES[c]?.bg};border-color:${COLOR_STYLES[c]?.bg};color:${COLOR_STYLES[c]?.text ?? '#fff'}` : ''"
      :aria-pressed="isActive(c)"
      @click="toggle(c)"
    >
      <span
        class="inline-block w-2.5 h-2.5 rounded-full border border-black/20 flex-shrink-0"
        :style="`background:${COLOR_STYLES[c]?.bg}`"
      ></span>
      {{ c }}
    </button>
  </div>
</template>

<script setup>
import { WINE_COLORS } from '@/utils/drinkStatus'
import { COLOR_STYLES } from '@/utils/wineColor'

const props = defineProps({
  modelValue: { type: Array, default: () => [] },
  label:      { type: String, default: 'Filter by color' },
})
const emit = defineEmits(['update:modelValue'])

function isActive(color) { return props.modelValue.includes(color) }
function toggle(color) {
  emit('update:modelValue', isActive(color)
    ? props.modelValue.filter(c => c !== color)
    : [...props.modelValue, color]
  )
}
</script>
