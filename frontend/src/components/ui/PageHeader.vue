<template>
  <div class="page-header">
    <div>
      <span v-if="eyebrow" class="label-eyebrow">{{ eyebrow }}</span>
      <h1 class="font-heading text-3xl font-bold text-base-content">{{ title }}</h1>
    </div>
    <slot />
  </div>
</template>

<script setup>
defineProps({
  eyebrow: { type: String, default: '' },
  title:   { type: String, required: true },
})
</script>
