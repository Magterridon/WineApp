<template>
  <div class="flex gap-2">
    <div class="relative flex-1">
      <svg
        xmlns="http://www.w3.org/2000/svg"
        class="absolute left-3 top-1/2 -translate-y-1/2 text-base-content/30 w-4 h-4 pointer-events-none"
        fill="currentColor"
        viewBox="0 0 16 16"
      >
        <path d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.099zm-5.242 1.656a5.5 5.5 0 1 1 0-11 5.5 5.5 0 0 1 0 11z"/>
      </svg>
      <input
        type="text"
        class="input input-bordered w-full pl-9"
        :value="modelValue"
        :placeholder="placeholder"
        @input="$emit('update:modelValue', $event.target.value)"
      />
    </div>
    <button
      v-if="modelValue"
      class="btn btn-ghost border border-base-300"
      type="button"
      @click="$emit('update:modelValue', '')"
    >✕</button>
  </div>
</template>

<script setup>
defineProps({
  modelValue:  { type: String, default: '' },
  placeholder: { type: String, default: 'Search...' },
})
defineEmits(['update:modelValue'])
</script>
