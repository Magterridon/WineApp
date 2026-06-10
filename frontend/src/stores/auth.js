import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '../services/authService'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const token = ref(null)
  const loading = ref(false)
  const error = ref(null)
  const initialized = ref(false)

  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => user.value?.role === 'Admin')

  function init() {
    if (initialized.value) return
    const storedToken = localStorage.getItem('wca_token')
    const storedUser = localStorage.getItem('wca_user')
    if (storedToken && storedUser) {
      token.value = storedToken
      user.value = JSON.parse(storedUser)
    }
    initialized.value = true
  }

  async function login(email, password) {
    loading.value = true
    error.value = null
    try {
      const result = await authService.login(email, password)
      token.value = result.token
      user.value = result.user
      localStorage.setItem('wca_token', result.token)
      localStorage.setItem('wca_user', JSON.stringify(result.user))
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      loading.value = false
    }
  }

  async function register(email, password) {
    loading.value = true
    error.value = null
    try {
      const result = await authService.register(email, password)
      token.value = result.token
      user.value = result.user
      localStorage.setItem('wca_token', result.token)
      localStorage.setItem('wca_user', JSON.stringify(result.user))
    } catch (err) {
      error.value = err.message
      throw err
    } finally {
      loading.value = false
    }
  }

  function logout() {
    token.value = null
    user.value = null
    error.value = null
    localStorage.removeItem('wca_token')
    localStorage.removeItem('wca_user')
  }

  return { user, token, loading, error, initialized, isAuthenticated, isAdmin, init, login, register, logout }
})
