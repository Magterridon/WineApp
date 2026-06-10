<template>
  <nav class="navbar navbar-expand-lg navbar-dark" style="background-color: #4a1020;">
    <div class="container">
      <router-link class="navbar-brand fw-bold" to="/">🍷 WineCellar</router-link>

      <button class="navbar-toggler" type="button" @click="isOpen = !isOpen" aria-label="Toggle navigation">
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="navbar-collapse" :class="{ show: isOpen, collapse: !isOpen }">
        <ul class="navbar-nav ms-auto align-items-lg-center">
          <template v-if="authStore.isAuthenticated">
            <li class="nav-item">
              <router-link class="nav-link" to="/cellar" @click="isOpen = false">My Cellar</router-link>
            </li>
            <li class="nav-item">
              <router-link class="nav-link" to="/wines" @click="isOpen = false">Wines</router-link>
            </li>
            <li class="nav-item">
              <router-link class="nav-link" to="/recipes" @click="isOpen = false">Recipes</router-link>
            </li>
            <li v-if="authStore.isAdmin" class="nav-item">
              <router-link class="nav-link" to="/admin/wines" @click="isOpen = false">
                <span class="badge bg-danger me-1">Admin</span>Catalog
              </router-link>
            </li>
            <li class="nav-item ms-lg-2">
              <span class="nav-link text-light opacity-75 small d-none d-lg-block">{{ authStore.user?.email }}</span>
            </li>
            <li class="nav-item">
              <button class="btn btn-outline-light btn-sm ms-lg-2 mt-2 mt-lg-0" @click="logout">Logout</button>
            </li>
          </template>
          <template v-else>
            <li class="nav-item">
              <router-link class="nav-link" to="/login" @click="isOpen = false">Login / Register</router-link>
            </li>
          </template>
        </ul>
      </div>
    </div>
  </nav>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const router = useRouter()
const isOpen = ref(false)

function logout() {
  authStore.logout()
  isOpen.value = false
  router.push('/login')
}
</script>
