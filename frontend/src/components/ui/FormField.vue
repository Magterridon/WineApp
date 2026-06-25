<template>
  <div class="space-y-1.5">
    <label v-if="label" class="block text-sm font-semibold text-base-content/70">
      {{ label }}<span v-if="required" class="text-error ml-0.5" aria-hidden="true">*</span>
    </label>
    <slot />
    <p v-if="error" class="text-xs text-error">{{ error }}</p>
    <p v-else-if="hint" class="text-xs text-base-content/40">{{ hint }}</p>
  </div>
</template>

<script setup>
defineProps({
  label:    { type: String, default: '' },
  error:    { type: String, default: '' },
  hint:     { type: String, default: '' },
  required: { type: Boolean, default: false },
})
</script>
