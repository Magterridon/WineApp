<template>
  <div class="card h-100 shadow-sm" data-testid="recipe-card">
    <img
      :src="recipe.imageUrl || 'https://placehold.co/300x200/5D4037/white?text=Recipe'"
      class="card-img-top"
      :alt="recipe.name"
      style="height: 150px; object-fit: cover;"
    />
    <div class="card-body d-flex flex-column">
      <span class="badge mb-2 text-white" style="background-color: #4a1020; width: fit-content;">{{ recipe.recipeType }}</span>
      <h6 class="card-title fw-semibold mb-1">{{ recipe.name }}</h6>
      <p class="card-text small text-muted flex-grow-1" style="overflow: hidden; display: -webkit-box; -webkit-line-clamp: 3; -webkit-box-orient: vertical;">
        {{ recipe.description }}
      </p>
      <div class="mt-auto pt-2">
        <router-link :to="`/recipes/${recipe.id}`" class="btn btn-outline-secondary btn-sm">View Recipe</router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  recipe: { type: Object, required: true }
})
</script>
