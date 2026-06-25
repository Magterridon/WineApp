<template>
  <div
    class="group flex flex-col bg-base-100 rounded-2xl border border-base-200 overflow-hidden shadow-sm hover:shadow-lg transition-all duration-300 hover:-translate-y-0.5"
    data-testid="wine-card"
  >
    <!-- Portrait image — wine bottle feel -->
    <figure class="relative overflow-hidden bg-base-200 flex-shrink-0" style="aspect-ratio:2/3">
      <img
        :src="wine.imageUrl || 'https://placehold.co/400x600/4a1020/faf8f5?text=🍷'"
        :alt="wine.name"
        class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
      />

      <!-- Gradient overlay -->
      <div class="absolute inset-x-0 bottom-0 h-20 bg-gradient-to-t from-black/50 to-transparent pointer-events-none"></div>

      <!-- Drink status — top right -->
      <div v-if="drinkStatus" class="absolute top-3 right-3">
        <span class="badge-pill backdrop-blur-md bg-black/40 text-white border border-white/15">
          {{ drinkStatus.label }}
        </span>
      </div>

      <!-- Color — bottom left -->
      <div v-if="wine.color" class="absolute bottom-3 left-3">
        <WineColorBadge :color="wine.color" />
      </div>
    </figure>

    <!-- Content -->
    <div class="flex flex-col flex-1 p-4 gap-2">
      <div>
        <h3 class="font-heading font-bold text-[15px] leading-snug line-clamp-1">{{ wine.name }}</h3>
        <p class="text-xs text-base-content/50 mt-0.5 truncate">{{ wine.domain }} · {{ wine.year }}</p>
      </div>

      <StarRating :rank="wine.rank" />

      <p class="text-[11px] text-base-content/40 italic line-clamp-1 min-h-[1rem]">
        {{ wine.cepages?.map(c => c.name).join(', ') || '' }}
      </p>

      <p class="text-xs text-base-content/50 line-clamp-2 flex-1 leading-relaxed min-h-[2rem]">
        {{ wine.description }}
      </p>

      <!-- Actions -->
      <div class="pt-3 mt-1 border-t border-base-200 flex gap-2">
        <router-link :to="`/wines/${wine.id}`" class="btn btn-sm btn-ghost border border-base-300 flex-1">
          Details
        </router-link>

        <button
          v-if="!inCellar"
          class="btn btn-sm btn-primary flex-1"
          data-testid="add-to-cellar-btn"
          @click="$emit('add-to-cellar', wine.id)"
        >
          + Cellar
        </button>

        <template v-else>
          <button
            class="btn btn-sm btn-secondary"
            data-testid="drink-btn"
            @click="$emit('drink', wine.id)"
          >Drink</button>
          <button
            class="btn btn-sm btn-ghost text-error border border-error/20"
            data-testid="remove-from-cellar-btn"
            @click="$emit('remove-from-cellar', wine.id)"
          >✕</button>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { getDrinkStatus } from '@/utils/drinkStatus'
import WineColorBadge from '@/components/ui/WineColorBadge.vue'
import StarRating     from '@/components/ui/StarRating.vue'

const props = defineProps({
  wine:     { type: Object,  required: true },
  inCellar: { type: Boolean, default: false },
})
defineEmits(['add-to-cellar', 'remove-from-cellar', 'drink'])

const drinkStatus = computed(() => getDrinkStatus(props.wine))
</script>
