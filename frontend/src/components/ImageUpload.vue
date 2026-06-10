<template>
  <div class="image-upload">

    <!-- Drop zone / preview -->
    <div
      class="drop-zone"
      :class="{ 'is-dragging': isDragging, 'has-image': previewUrl && !uploading }"
      @dragover.prevent="isDragging = true"
      @dragleave.prevent="isDragging = false"
      @drop.prevent="onDrop"
      @click="fileInput.click()"
      role="button"
      tabindex="0"
      :aria-label="previewUrl ? 'Change image' : 'Upload image'"
      @keydown.enter.prevent="fileInput.click()"
    >
      <!-- Uploading spinner -->
      <div v-if="uploading" class="drop-zone-placeholder">
        <div class="spinner-border text-secondary mb-2" style="width:2rem;height:2rem"></div>
        <p class="mb-0 small text-muted">Uploading…</p>
      </div>

      <!-- Preview image -->
      <img
        v-else-if="previewUrl"
        :src="previewUrl"
        alt="Image preview"
        class="drop-zone-preview"
        @error="onPreviewError"
      />

      <!-- Empty state -->
      <div v-else class="drop-zone-placeholder">
        <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" fill="none" viewBox="0 0 24 24" stroke="currentColor" class="text-muted mb-2" style="opacity:.4">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
            d="M3 16.5V18a2.25 2.25 0 002.25 2.25h13.5A2.25 2.25 0 0021 18v-1.5M16.5 12L12 7.5m0 0L7.5 12M12 7.5V18" />
        </svg>
        <p class="mb-0 small">Drag &amp; drop an image here</p>
        <p class="mb-0 small text-muted">or click to browse</p>
      </div>

      <!-- Change overlay on hover when image is set -->
      <div v-if="previewUrl && !uploading" class="drop-zone-overlay">
        <span class="small">Change image</span>
      </div>
    </div>

    <!-- Hidden file input -->
    <input
      ref="fileInput"
      type="file"
      accept="image/jpeg,image/png,image/gif,image/webp"
      class="d-none"
      @change="onFileSelected"
    />

    <!-- URL text input -->
    <div class="mt-2 d-flex gap-2 align-items-center">
      <input
        v-model="urlInput"
        type="url"
        class="form-control form-control-sm"
        placeholder="or paste an image URL…"
        @blur="applyUrl"
        @keyup.enter="applyUrl"
      />
      <button
        v-if="modelValue"
        type="button"
        class="btn btn-sm btn-outline-danger flex-shrink-0"
        title="Remove image"
        @click.stop="clear"
      >✕</button>
    </div>

    <div v-if="error" class="text-danger small mt-1">{{ error }}</div>
    <div v-if="modelValue" class="text-muted small mt-1 text-truncate" style="max-width:100%">{{ modelValue }}</div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { wineService } from '@/services/wineService'

const props = defineProps({
  modelValue: { type: String, default: '' }
})
const emit = defineEmits(['update:modelValue'])

const fileInput  = ref(null)
const isDragging = ref(false)
const uploading  = ref(false)
const error      = ref('')
const urlInput   = ref(props.modelValue || '')
const previewUrl = ref(props.modelValue || '')

// Keep urlInput and previewUrl in sync when parent changes modelValue
watch(() => props.modelValue, val => {
  urlInput.value  = val || ''
  previewUrl.value = val || ''
})

function onPreviewError() {
  previewUrl.value = ''
}

function applyUrl() {
  const url = urlInput.value.trim()
  previewUrl.value = url
  emit('update:modelValue', url)
  error.value = ''
}

function clear() {
  urlInput.value   = ''
  previewUrl.value = ''
  error.value      = ''
  emit('update:modelValue', '')
}

function onDrop(e) {
  isDragging.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) {
    uploadFile(file)
  } else {
    // Maybe a URL was dropped (e.g. from browser image)
    const url = e.dataTransfer?.getData('text/uri-list') || e.dataTransfer?.getData('text/plain') || ''
    if (url.match(/^https?:\/\//)) {
      urlInput.value   = url
      previewUrl.value = url
      emit('update:modelValue', url)
    }
  }
}

function onFileSelected(e) {
  const file = e.target.files?.[0]
  if (file) uploadFile(file)
  // Reset so the same file can be re-selected
  e.target.value = ''
}

async function uploadFile(file) {
  error.value = ''
  uploading.value = true
  // Show local preview immediately while uploading
  const objectUrl = URL.createObjectURL(file)
  previewUrl.value = objectUrl
  try {
    const imageUrl = await wineService.uploadImage(file)
    URL.revokeObjectURL(objectUrl)
    previewUrl.value = imageUrl
    urlInput.value   = imageUrl
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

<style scoped>
.drop-zone {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 140px;
  border: 2px dashed #ced4da;
  border-radius: 8px;
  cursor: pointer;
  overflow: hidden;
  transition: border-color .15s, background .15s;
  background: #fafafa;
}
.drop-zone:hover,
.drop-zone:focus {
  border-color: #6c757d;
  background: #f5f5f5;
  outline: none;
}
.drop-zone.is-dragging {
  border-color: #4a1020;
  background: #fff5f7;
}
.drop-zone.has-image {
  border-style: solid;
  border-color: #dee2e6;
  background: #000;
}
.drop-zone-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 1rem;
  text-align: center;
  color: #6c757d;
  pointer-events: none;
}
.drop-zone-preview {
  width: 100%;
  height: 140px;
  object-fit: cover;
  display: block;
}
.drop-zone-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0,0,0,.45);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  opacity: 0;
  transition: opacity .15s;
}
.drop-zone:hover .drop-zone-overlay {
  opacity: 1;
}
</style>
