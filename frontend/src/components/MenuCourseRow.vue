<template>
  <div class="rounded-2xl overflow-hidden border border-base-300 shadow-sm">
    <div class="grid grid-cols-1 lg:grid-cols-5">

      <!-- Wine panel (dark) -->
      <div class="lg:col-span-2 flex flex-col" style="background: linear-gradient(160deg, #1c0a0a 0%, #3a1020 100%); color:#fff;">
        <div class="bg-black/30 overflow-hidden flex-shrink-0">
          <img :src="wineImage" :alt="wine ? wine.name : 'Wine'" class="w-full h-48 object-cover opacity-90 hover:opacity-100 transition-opacity" />
        </div>

        <div v-if="wine" class="p-5 flex flex-col flex-1 gap-2">
          <div class="flex items-start justify-between gap-2">
            <h3 class="font-heading font-bold text-lg leading-tight text-white">{{ wine.name }}</h3>
            <span class="text-white/50 text-sm font-semibold flex-shrink-0">{{ wine.year }}</span>
          </div>
          <p class="text-white/70 text-sm">{{ wine.domain }}</p>
          <p v-if="wineLocation" class="text-white/40 text-xs">{{ wineLocation }}</p>

          <div class="flex flex-wrap gap-1.5 mt-1">
            <WineColorBadge v-if="wine.color" :color="wine.color" />
            <DrinkStatusBadge v-if="drinkStatus" :status="drinkStatus" class="!bg-white/10 !text-white/70 !border-white/20" />
            <span v-if="wine.rank" class="text-amber-300 text-sm">{{ stars }}</span>
          </div>

          <p v-if="cepageText" class="text-white/40 text-xs italic mt-1">{{ cepageText }}</p>
          <p v-if="drinkWindow" class="text-white/35 text-xs">{{ drinkWindow }}</p>
          <p v-if="wine.description" class="text-white/55 text-sm leading-relaxed mt-auto pt-2">{{ wine.description }}</p>
        </div>

        <div v-else class="flex-1 flex items-center justify-center p-6 text-center">
          <div>
            <div class="text-4xl opacity-25 mb-2">🍾</div>
            <p class="text-white/40 text-sm mb-3">No cellar wine available</p>
            <router-link to="/wines" class="btn btn-xs btn-ghost border border-white/20 text-white/50">Browse Wines</router-link>
          </div>
        </div>
      </div>

      <!-- Recipe panel (light) -->
      <div class="lg:col-span-3 bg-base-100 p-5 lg:p-8 flex flex-col">
        <div class="label-caps border-b border-primary/10 pb-2 mb-4">{{ label }}</div>

        <template v-if="recipe">
          <h2 class="font-heading text-2xl font-bold text-base-content leading-tight mb-2">{{ recipe.name }}</h2>
          <p v-if="recipe.description" class="text-base-content/60 text-sm leading-relaxed mb-5">{{ recipe.description }}</p>

          <div v-if="recipe.ingredients?.length" class="flex-1">
            <p class="label-caps mb-2">Ingredients</p>
            <ul class="space-y-0">
              <li
                v-for="(ing, i) in recipe.ingredients"
                :key="i"
                class="relative pl-4 py-1.5 text-sm text-base-content/70 border-b border-base-200 last:border-0
                       before:content-['·'] before:absolute before:left-0 before:top-1.5 before:text-primary before:font-bold"
              >{{ ing }}</li>
            </ul>
          </div>

          <div class="mt-auto pt-5">
            <router-link
              :to="`/recipes/${recipe.id}`"
              class="text-sm font-semibold text-primary border-b border-primary/30 pb-px hover:border-primary transition-colors no-underline"
            >See full recipe →</router-link>
          </div>
        </template>

        <template v-else>
          <p class="text-base-content/40 italic text-sm mb-3">No recipe available for this course yet.</p>
          <router-link to="/recipes" class="btn btn-sm btn-ghost border border-base-300 self-start">Add Meals</router-link>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { getDrinkStatus } from '@/utils/drinkStatus'
import WineColorBadge  from '@/components/ui/WineColorBadge.vue'
import DrinkStatusBadge from '@/components/ui/DrinkStatusBadge.vue'

const props = defineProps({
  label:  { type: String, required: true },
  wine:   { type: Object, default: null },
  recipe: { type: Object, default: null },
})

const drinkStatus  = computed(() => props.wine ? getDrinkStatus(props.wine) : null)
const cepageText   = computed(() => { const c = props.wine?.cepages; return c?.length ? c.map(c => c.percentage ? `${c.name} ${c.percentage}%` : c.name).join(' · ') : '' })
const stars        = computed(() => { const r = props.wine?.rank; return r ? '★'.repeat(r) + '☆'.repeat(5 - r) : '' })
const wineLocation = computed(() => props.wine ? [props.wine.appellation, props.wine.region, props.wine.country].filter(Boolean).join(' · ') : '')
const drinkWindow  = computed(() => { const from = props.wine?.drinkFromYear, to = props.wine?.drinkToYear; return (from || to) ? `Drink ${from ?? '?'} – ${to ?? '?'}` : '' })
const wineImage    = computed(() => props.wine?.imageUrl || 'https://placehold.co/480x220/2d1515/white?text=🍷')
</script>
