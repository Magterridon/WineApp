<template>
  <div class="min-h-screen bg-base-100 flex flex-col">
    <AppNavbar />
    <main class="flex-1 w-full max-w-6xl mx-auto px-4 py-6 pb-24 md:pb-6">
      <RouterView />
    </main>
    <AppBottomNav v-if="authStore.isAuthenticated" />
  </div>
</template>

<script setup>
import AppNavbar from '@/components/AppNavbar.vue'
import AppBottomNav from '@/components/AppBottomNav.vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
</script>
