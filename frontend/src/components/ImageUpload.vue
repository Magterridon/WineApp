<template>
  <div>
    <!-- Drop zone -->
    <div
      class="relative flex items-center justify-center min-h-[140px] border-2 border-dashed rounded-xl cursor-pointer overflow-hidden transition-all duration-150"
      :class="isDragging ? 'border-primary bg-primary/5' : 'border-base-300 hover:border-primary/50 bg-base-200/40'"
      role="button"
      tabindex="0"
      :aria-label="previewUrl ? 'Change image' : 'Upload image'"
      @dragover.prevent="isDragging = true"
      @dragleave.prevent="isDragging = false"
      @drop.prevent="onDrop"
      @click="fileInput.click()"
      @keydown.enter.prevent="fileInput.click()"
    >
      <!-- Uploading -->
      <div v-if="uploading" class="flex flex-col items-center gap-2 text-base-content/50">
        <span class="loading loading-spinner loading-md text-primary"></span>
        <p class="text-sm">Uploading…</p>
      </div>

      <!-- Preview -->
      <template v-else-if="previewUrl">
        <img :src="previewUrl" alt="Preview" class="w-full h-36 object-cover" @error="previewUrl = ''" />
        <div class="absolute inset-0 bg-black/40 opacity-0 hover:opacity-100 transition-opacity flex items-center justify-center">
          <span class="text-white text-sm font-medium">Change image</span>
        </div>
      </template>

      <!-- Empty state -->
      <div v-else class="flex flex-col items-center gap-2 py-4 text-base-content/40 pointer-events-none">
        <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
            d="M3 16.5V18a2.25 2.25 0 002.25 2.25h13.5A2.25 2.25 0 0021 18v-1.5M16.5 12L12 7.5m0 0L7.5 12M12 7.5V18" />
        </svg>
        <p class="text-sm text-center">Drag &amp; drop or click to browse</p>
        <p class="text-xs">PNG, JPG, WebP</p>
      </div>
    </div>

    <input ref="fileInput" type="file" accept="image/jpeg,image/png,image/gif,image/webp" class="hidden" @change="onFileSelected" />

    <!-- URL fallback -->
    <div class="flex gap-2 mt-2">
      <input
        v-model="urlInput"
        type="url"
        class="input input-bordered input-sm flex-1 text-xs"
        placeholder="Or paste an image URL…"
        @blur="applyUrl"
        @keyup.enter="applyUrl"
      />
      <button v-if="modelValue" type="button" class="btn btn-sm btn-ghost text-error" title="Remove" @click.stop="clear">✕</button>
    </div>

    <div v-if="error" class="text-error text-xs mt-1">{{ error }}</div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { wineService } from '@/services/wineService'

const props = defineProps({ modelValue: { type: String, default: '' } })
const emit  = defineEmits(['update:modelValue'])

const fileInput  = ref(null)
const isDragging = ref(false)
const uploading  = ref(false)
const error      = ref('')
const urlInput   = ref(props.modelValue || '')
const previewUrl = ref(props.modelValue || '')

watch(() => props.modelValue, val => {
  urlInput.value   = val || ''
  previewUrl.value = val || ''
})

function applyUrl() {
  const url = urlInput.value.trim()
  previewUrl.value = url
  emit('update:modelValue', url)
  error.value = ''
}

function clear() {
  urlInput.value = previewUrl.value = ''
  error.value = ''
  emit('update:modelValue', '')
}

function onDrop(e) {
  isDragging.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) { uploadFile(file); return }
  const url = e.dataTransfer?.getData('text/uri-list') || e.dataTransfer?.getData('text/plain') || ''
  if (url.match(/^https?:\/\//)) {
    urlInput.value = previewUrl.value = url
    emit('update:modelValue', url)
  }
}

function onFileSelected(e) {
  const file = e.target.files?.[0]
  if (file) uploadFile(file)
  e.target.value = ''
}

async function uploadFile(file) {
  error.value = ''
  uploading.value = true
  const objectUrl = URL.createObjectURL(file)
  previewUrl.value = objectUrl
  try {
    const imageUrl = await wineService.uploadImage(file)
    URL.revokeObjectURL(objectUrl)
    previewUrl.value = urlInput.value = imageUrl
    emit('update:modelValue', imageUrl)
  } catch (err) {
    URL.revokeObjectURL(objectUrl)
    previewUrl.value = props.modelValue || ''
    error.value = err.message || 'Upload failed'
  } finally {
    uploading.value = false
  }
}
</script>
