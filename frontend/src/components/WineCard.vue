<template>
  <div class="card h-100 shadow-sm" data-testid="wine-card">
    <div style="position: relative;">
      <img
        :src="wine.imageUrl || 'https://placehold.co/300x200/4a1020/white?text=Wine'"
        class="card-img-top"
        :alt="wine.name"
        style="height: 150px; object-fit: cover;"
      />
      <span
        v-if="drinkStatus"
        class="badge position-absolute top-0 end-0 m-2"
        :class="`bg-${drinkStatus.bg}`"
        style="font-size: 0.7rem;"
      >{{ drinkStatus.label }}</span>
    </div>
    <div class="card-body d-flex flex-column">
      <div class="d-flex justify-content-between align-items-start mb-1">
        <h6 class="card-title mb-0 fw-semibold">{{ wine.name }}</h6>
        <span
          v-if="wine.color"
          class="badge ms-1 flex-shrink-0"
          :style="colorStyle"
          style="font-size: 0.65rem;"
        >{{ wine.color }}</span>
      </div>
      <p class="text-muted small mb-1">{{ wine.domain }} · {{ wine.year }}</p>
      <p class="small mb-1 text-warning">{{ stars(wine.rank) }}</p>
      <p
        class="card-text small text-muted mb-2 flex-grow-1"
        style="overflow: hidden; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical;"
      >{{ wine.description }}</p>
      <p class="small text-muted mb-3 fst-italic">
        {{ wine.cepages.map(c => c.percentage ? `${c.name} ${c.percentage}%` : c.name).join(', ') }}
      </p>
      <div class="d-flex gap-2 mt-auto">
        <router-link :to="`/wines/${wine.id}`" class="btn btn-outline-secondary btn-sm">Details</router-link>
        <button
          v-if="!inCellar"
          class="btn btn-sm text-white"
          style="background-color: #4a1020;"
          data-testid="add-to-cellar-btn"
          @click="$emit('add-to-cellar', wine.id)"
        >+ Cellar</button>
        <template v-else>
          <button
            class="btn btn-sm text-white"
            style="background-color: #722f37;"
            data-testid="drink-btn"
            @click="$emit('drink', wine.id)"
          >Drink</button>
          <button
            class="btn btn-outline-danger btn-sm"
            data-testid="remove-from-cellar-btn"
            @click="$emit('remove-from-cellar', wine.id)"
          >Remove</button>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { getDrinkStatus, COLOR_STYLES } from '@/utils/drinkStatus'

const props = defineProps({
  wine: { type: Object, required: true },
  inCellar: { type: Boolean, default: false }
})
defineEmits(['add-to-cellar', 'remove-from-cellar', 'drink'])

const drinkStatus = computed(() => getDrinkStatus(props.wine))
const colorStyle = computed(() => {
  const s = COLOR_STYLES[props.wine.color]
  return s ? `background-color: ${s.bg}; color: ${s.text};` : ''
})

function stars(rank) {
  return '★'.repeat(rank) + '☆'.repeat(5 - rank)
}
</script>
