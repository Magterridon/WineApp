<template>
  <div
    class="group flex flex-col bg-base-100 rounded-2xl overflow-hidden shadow-sm hover:shadow-lg transition-all duration-300 hover:-translate-y-0.5"
    data-testid="recipe-card"
  >
    <!-- Square image — food photography format -->
    <figure class="relative overflow-hidden bg-base-200 flex-shrink-0" style="aspect-ratio:1/1">
      <img
        :src="recipe.imageUrl || 'https://placehold.co/400x400/5D4037/faf8f5?text=🍽️'"
        :alt="recipe.name"
        class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
      />
      <!-- Course type — top left -->
      <div class="absolute top-3 left-3">
        <span
          class="badge-pill backdrop-blur-md bg-black/40 text-white border border-white/15"
        >{{ recipe.recipeType }}</span>
      </div>
    </figure>

    <!-- Content -->
    <div class="flex flex-col flex-1 p-4 gap-2">
      <h3 class="font-heading font-bold text-[15px] leading-snug line-clamp-2">
        {{ recipe.name }}
      </h3>

      <p class="text-xs text-base-content/50 line-clamp-3 flex-1 leading-relaxed">
        {{ recipe.description }}
      </p>

      <div class="pt-3 mt-1 border-t border-base-200">
        <router-link
          :to="`/recipes/${recipe.id}`"
          class="btn btn-xs btn-ghost border border-base-300 w-full"
        >
          View Recipe
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({ recipe: { type: Object, required: true } })
</script>
