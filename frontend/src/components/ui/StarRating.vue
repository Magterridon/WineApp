<template>
  <span
    class="inline-flex items-center gap-px"
    role="img"
    :aria-label="`${rank} out of 5 stars`"
  >
    <span
      v-for="n in 5"
      :key="n"
      class="text-sm leading-none select-none"
      :class="n <= rank ? 'text-amber-400' : 'text-base-content/20'"
    >★</span>
  </span>
</template>

<script setup>
defineProps({
  rank: { type: Number, default: 0 },
})
</script>
