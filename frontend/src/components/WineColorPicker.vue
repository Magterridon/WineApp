<template>
  <div class="wine-color-picker d-flex flex-wrap gap-2" role="group" :aria-label="label">
    <button
      v-for="c in WINE_COLORS"
      :key="c"
      type="button"
      class="btn btn-sm wine-color-btn d-inline-flex align-items-center gap-1"
      :class="isActive(c) ? 'active' : 'btn-outline-secondary'"
      :style="isActive(c) ? `background:${COLOR_STYLES[c]?.bg};border-color:${COLOR_STYLES[c]?.bg};color:${COLOR_STYLES[c]?.text ?? '#fff'}` : ''"
      :aria-pressed="isActive(c)"
      @click="toggle(c)"
    >
      <!-- Simple bottle outline SVG -->
      <svg width="9" height="14" viewBox="0 0 9 14" aria-hidden="true" fill="none">
        <path
          d="M3.2 0 h2.6 v2.1 c1.4 .5 2.2 1.7 2.2 3 V10.5 a2.8 2.8 0 0 1-5.6 0 V5.1 c0-1.3.8-2.5 2.2-3 V0 z"
          :fill="isActive(c) ? (COLOR_STYLES[c]?.text === '#333' ? 'rgba(0,0,0,.25)' : 'rgba(255,255,255,.35)') : COLOR_STYLES[c]?.bg"
          :stroke="isActive(c) ? (COLOR_STYLES[c]?.text === '#333' ? 'rgba(0,0,0,.5)' : 'rgba(255,255,255,.7)') : COLOR_STYLES[c]?.bg"
          stroke-width="0.6"
        />
      </svg>
      <!-- Color swatch circle -->
      <span
        aria-hidden="true"
        style="width:9px;height:9px;border-radius:50%;display:inline-block;border:1px solid rgba(0,0,0,.25);flex-shrink:0"
        :style="`background-color:${COLOR_STYLES[c]?.bg};`"
      ></span>
      <span>{{ c }}</span>
    </button>
  </div>
</template>

<script setup>
import { WINE_COLORS, COLOR_STYLES } from '@/utils/drinkStatus'

const props = defineProps({
  modelValue: { type: Array, default: () => [] },
  label:      { type: String, default: 'Filter by color' }
})
const emit = defineEmits(['update:modelValue'])

function isActive(color) {
  return props.modelValue.includes(color)
}

function toggle(color) {
  const updated = isActive(color)
    ? props.modelValue.filter(c => c !== color)
    : [...props.modelValue, color]
  emit('update:modelValue', updated)
}
</script>

<style scoped>
.wine-color-btn {
  font-size: 0.78rem;
  padding: 3px 8px;
  transition: all 0.15s ease;
}
.wine-color-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 6px rgba(0,0,0,.15);
}
.wine-color-btn.active {
  box-shadow: 0 0 0 2px rgba(0,0,0,.2), 0 2px 6px rgba(0,0,0,.15);
}
</style>
