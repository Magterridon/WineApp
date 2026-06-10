<template>
  <div v-if="message" :class="`alert alert-${type} alert-dismissible`" role="alert">
    {{ message }}
    <button v-if="dismissible" type="button" class="btn-close" @click="$emit('dismiss')"></button>
  </div>
</template>

<script setup>
defineProps({
  message: { type: String, default: '' },
  type: { type: String, default: 'danger' },
  dismissible: { type: Boolean, default: true }
})
defineEmits(['dismiss'])
</script>
