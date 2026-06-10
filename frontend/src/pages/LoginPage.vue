<template>
  <div class="row justify-content-center mt-4">
    <div class="col-md-5 col-lg-4">
      <div class="text-center mb-4">
        <div style="font-size: 3rem;">🍷</div>
        <h2 class="fw-bold">WineCellar</h2>
        <p class="text-muted">Your personal wine collection</p>
      </div>

      <div class="card shadow-sm">
        <div class="card-header p-0">
          <ul class="nav nav-tabs card-header-tabs">
            <li class="nav-item w-50 text-center">
              <button
                class="nav-link w-100"
                :class="{ active: mode === 'login' }"
                @click="mode = 'login'; error = ''"
                data-testid="login-tab"
              >Login</button>
            </li>
            <li class="nav-item w-50 text-center">
              <button
                class="nav-link w-100"
                :class="{ active: mode === 'register' }"
                @click="mode = 'register'; error = ''"
                data-testid="register-tab"
              >Register</button>
            </li>
          </ul>
        </div>

        <div class="card-body p-4">
          <AlertMessage :message="error" @dismiss="error = ''" />
          <AlertMessage :message="successMsg" type="success" :dismissible="false" />

          <form @submit.prevent="handleSubmit">
            <div class="mb-3">
              <label class="form-label fw-semibold">Email address</label>
              <input
                v-model="email"
                type="email"
                class="form-control"
                :class="{ 'is-invalid': fieldErrors.email }"
                placeholder="you@example.com"
                autocomplete="email"
                data-testid="email-input"
              />
              <div class="invalid-feedback">{{ fieldErrors.email }}</div>
            </div>

            <div class="mb-3">
              <label class="form-label fw-semibold">Password</label>
              <input
                v-model="password"
                type="password"
                class="form-control"
                :class="{ 'is-invalid': fieldErrors.password }"
                placeholder="••••••••"
                autocomplete="current-password"
                data-testid="password-input"
              />
              <div class="invalid-feedback">{{ fieldErrors.password }}</div>
            </div>

            <button
              type="submit"
              class="btn w-100 text-white"
              style="background-color: #4a1020;"
              :disabled="authStore.loading"
              :data-testid="mode === 'login' ? 'login-btn' : 'register-btn'"
            >
              <span v-if="authStore.loading" class="spinner-border spinner-border-sm me-1"></span>
              {{ mode === 'login' ? 'Login' : 'Create Account' }}
            </button>
          </form>

        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AlertMessage from '@/components/AlertMessage.vue'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const mode = ref('login')
const email = ref('')
const password = ref('')
const error = ref('')
const successMsg = ref('')
const fieldErrors = reactive({})

function validate() {
  Object.keys(fieldErrors).forEach(k => delete fieldErrors[k])
  if (!email.value.trim()) fieldErrors.email = 'Email is required'
  else if (!email.value.includes('@')) fieldErrors.email = 'Enter a valid email'
  if (!password.value) fieldErrors.password = 'Password is required'
  else if (password.value.length < 6) fieldErrors.password = 'Password must be at least 6 characters'
  return Object.keys(fieldErrors).length === 0
}

async function handleSubmit() {
  if (!validate()) return
  error.value = ''
  successMsg.value = ''
  try {
    if (mode.value === 'login') {
      await authStore.login(email.value, password.value)
    } else {
      await authStore.register(email.value, password.value)
      successMsg.value = 'Account created! Redirecting...'
    }
    const redirect = route.query.redirect || '/cellar'
    router.push(redirect)
  } catch (err) {
    error.value = err.message
  }
}
</script>
