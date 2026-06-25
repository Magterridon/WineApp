<template>
  <div class="empty-state">
    <span v-if="icon" class="empty-state-icon">{{ icon }}</span>
    <h3 class="empty-state-title">{{ title }}</h3>
    <p v-if="body" class="empty-state-body">{{ body }}</p>
    <slot />
  </div>
</template>

<script setup>
defineProps({
  icon:  { type: String, default: '' },
  title: { type: String, required: true },
  body:  { type: String, default: '' },
})
</script>
