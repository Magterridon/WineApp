<template>
  <div
    v-if="message"
    role="alert"
    class="flex items-start gap-3 px-4 py-3 rounded-xl text-sm mb-4"
    :class="styles[type] ?? styles.danger"
  >
    <span class="shrink-0 mt-0.5">{{ icons[type] ?? icons.danger }}</span>
    <span class="flex-1">{{ message }}</span>
    <button
      v-if="dismissible"
      class="shrink-0 opacity-50 hover:opacity-100 transition-opacity leading-none ml-2"
      @click="$emit('dismiss')"
      aria-label="Dismiss"
    >✕</button>
  </div>
</template>

<script setup>
defineProps({
  message:     { type: String,  default: '' },
  type:        { type: String,  default: 'danger' },
  dismissible: { type: Boolean, default: true },
})
defineEmits(['dismiss'])

const styles = {
  danger:  'bg-error/10 text-error border border-error/20',
  success: 'bg-success/10 text-success border border-success/20',
  warning: 'bg-warning/10 text-warning border border-warning/20',
  info:    'bg-info/10 text-info border border-info/20',
}
const icons = {
  danger:  '⚠️',
  success: '✓',
  warning: '⚠️',
  info:    'ℹ️',
}
</script>
