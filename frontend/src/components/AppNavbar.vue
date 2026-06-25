<template>
  <header class="sticky top-0 z-40 bg-base-100/90 backdrop-blur border-b border-base-200">
    <div class="max-w-6xl mx-auto px-4 h-14 flex items-center justify-between gap-4">

      <!-- Brand -->
      <router-link
        to="/"
        class="flex items-center gap-2 font-heading font-semibold text-lg text-base-content no-underline shrink-0"
      >
        <span class="text-xl leading-none" aria-hidden="true">🍷</span>
        <span class="hidden sm:inline">WineCellar</span>
      </router-link>

      <!-- Desktop nav -->
      <nav v-if="authStore.isAuthenticated" class="hidden md:flex items-center gap-1" aria-label="Main navigation">
        <router-link
          v-for="link in navLinks"
          :key="link.to"
          :to="link.to"
          class="px-3 py-1.5 rounded-lg text-sm font-medium text-base-content/60 hover:text-base-content hover:bg-base-200 transition-colors"
          active-class="text-primary bg-primary/8 hover:bg-primary/8 hover:text-primary"
        >{{ link.label }}</router-link>

        <template v-if="authStore.isAdmin">
          <div class="w-px h-5 bg-base-300 mx-1.5" aria-hidden="true"></div>
          <router-link
            to="/admin/wines"
            class="px-3 py-1.5 rounded-lg text-sm font-medium text-base-content/50 hover:text-base-content hover:bg-base-200 transition-colors inline-flex items-center gap-1.5"
            active-class="text-primary bg-primary/8 hover:bg-primary/8 hover:text-primary"
          >
            <svg class="w-3 h-3 opacity-60 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <rect x="5" y="11" width="14" height="10" rx="2"/>
              <path d="M8 11V7a4 4 0 018 0v4"/>
            </svg>
            Catalog
          </router-link>
          <router-link
            to="/admin/pairing-rules"
            class="px-3 py-1.5 rounded-lg text-sm font-medium text-base-content/50 hover:text-base-content hover:bg-base-200 transition-colors inline-flex items-center gap-1.5"
            active-class="text-primary bg-primary/8 hover:bg-primary/8 hover:text-primary"
          >
            <svg class="w-3 h-3 opacity-60 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <rect x="5" y="11" width="14" height="10" rx="2"/>
              <path d="M8 11V7a4 4 0 018 0v4"/>
            </svg>
            Pairings
          </router-link>
        </template>
      </nav>

      <!-- Right: user + sign out -->
      <div class="flex items-center gap-2 shrink-0">
        <template v-if="authStore.isAuthenticated">
          <span class="hidden lg:block text-xs text-base-content/35 max-w-[160px] truncate">
            {{ authStore.user?.email }}
          </span>
          <button
            class="text-sm font-medium text-base-content/50 hover:text-base-content transition-colors px-2 py-1 rounded-lg hover:bg-base-200"
            @click="logout"
          >
            Sign out
          </button>
        </template>
        <template v-else>
          <router-link to="/login" class="btn btn-sm btn-primary">Sign in</router-link>
        </template>
      </div>

    </div>
  </header>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const router = useRouter()

const navLinks = [
  { to: '/cellar',      label: 'My Cellar' },
  { to: '/wines',       label: 'Wines' },
  { to: '/recipes',     label: 'Meals' },
  { to: '/weekly-menu', label: 'Weekly Menu' },
]

function logout() {
  authStore.logout()
  router.push('/login')
}
</script>
