<template>
  <span v-if="status" class="badge-pill" :class="statusClass">{{ status.label }}</span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  status: { type: Object, default: null }, // { label: string, bg: string }
})

const statusClass = computed(() => {
  const map = {
    success:   'bg-success/15 text-success border border-success/30',
    warning:   'bg-warning/15 text-warning border border-warning/30',
    danger:    'bg-error/15 text-error border border-error/30',
    secondary: 'bg-base-200 text-base-content/50 border border-base-300',
  }
  return map[props.status?.bg] ?? 'bg-base-200 text-base-content/50 border border-base-300'
})
</script>
