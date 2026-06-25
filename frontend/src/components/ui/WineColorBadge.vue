<template>
  <span v-if="colorStyle" class="badge-pill" :style="colorStyle">{{ color }}</span>
</template>

<script setup>
import { computed } from 'vue'
import { getWineColorStyle } from '@/utils/wineColor'

const props = defineProps({
  color: { type: String, default: '' },
})

const colorStyle = computed(() => getWineColorStyle(props.color))
</script>
