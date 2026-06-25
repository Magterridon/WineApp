<template>
  <nav
    class="fixed bottom-0 left-0 right-0 z-40 md:hidden bg-base-100/95 backdrop-blur border-t border-base-200"
    aria-label="Main navigation"
  >
    <div class="flex justify-around items-stretch h-16 max-w-lg mx-auto px-1">

      <!-- Cellar -->
      <router-link to="/cellar" class="bottom-nav-item" active-class="active">
        <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <rect x="3" y="3" width="18" height="18" rx="2"/>
          <line x1="3" y1="9" x2="21" y2="9"/>
          <line x1="3" y1="15" x2="21" y2="15"/>
          <line x1="9" y1="3" x2="9" y2="21"/>
          <line x1="15" y1="3" x2="15" y2="21"/>
        </svg>
        <span>Cellar</span>
      </router-link>

      <!-- Wines -->
      <router-link to="/wines" class="bottom-nav-item" active-class="active">
        <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <path d="M7 3h10L15 13a3.5 3.5 0 01-6 0L7 3z"/>
          <line x1="12" y1="16.5" x2="12" y2="21"/>
          <line x1="9" y1="21" x2="15" y2="21"/>
        </svg>
        <span>Wines</span>
      </router-link>

      <!-- Meals -->
      <router-link to="/recipes" class="bottom-nav-item" active-class="active">
        <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <path d="M8 3v5a3 3 0 006 0V3"/>
          <line x1="11" y1="8" x2="11" y2="21"/>
          <line x1="16" y1="3" x2="16" y2="21"/>
        </svg>
        <span>Meals</span>
      </router-link>

      <!-- Weekly Menu -->
      <router-link to="/weekly-menu" class="bottom-nav-item" active-class="active">
        <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <rect x="3" y="4" width="18" height="17" rx="2"/>
          <line x1="16" y1="2" x2="16" y2="6"/>
          <line x1="8" y1="2" x2="8" y2="6"/>
          <line x1="3" y1="10" x2="21" y2="10"/>
          <line x1="8" y1="14" x2="8" y2="14"/>
          <line x1="12" y1="14" x2="12" y2="14"/>
          <line x1="16" y1="14" x2="16" y2="14"/>
        </svg>
        <span>Menu</span>
      </router-link>

      <!-- Admin (if admin) -->
      <router-link v-if="authStore.isAdmin" to="/admin/wines" class="bottom-nav-item" active-class="active">
        <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <rect x="5" y="11" width="14" height="10" rx="2"/>
          <path d="M8 11V7a4 4 0 018 0v4"/>
          <circle cx="12" cy="16" r="1" fill="currentColor" stroke="none"/>
        </svg>
        <span>Admin</span>
      </router-link>

    </div>
  </nav>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
</script>
