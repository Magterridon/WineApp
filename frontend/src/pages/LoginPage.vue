<template>
  <div class="min-h-[85vh] flex items-center justify-center px-4 py-10">
    <div class="w-full max-w-sm">

      <!-- Brand mark -->
      <div class="text-center mb-10">
        <div class="text-5xl mb-4" style="filter: drop-shadow(0 2px 8px rgb(74 16 32 / 0.3))">🍷</div>
        <h1 class="font-heading text-4xl font-bold text-base-content tracking-tight">WineCellar</h1>
        <p class="text-base-content/40 text-sm mt-2 tracking-wide">Your personal wine collection</p>
      </div>

      <!-- Card -->
      <div class="bg-base-100 rounded-2xl shadow-md overflow-hidden border border-base-200">

        <!-- Tabs -->
        <div class="flex border-b border-base-200">
          <button
            class="flex-1 py-3.5 text-sm font-semibold transition-colors"
            :class="mode === 'login'
              ? 'text-primary border-b-2 border-primary bg-primary/5'
              : 'text-base-content/40 hover:text-base-content/70'"
            @click="mode = 'login'; error = ''"
            data-testid="login-tab"
          >Sign in</button>
          <button
            class="flex-1 py-3.5 text-sm font-semibold transition-colors"
            :class="mode === 'register'
              ? 'text-primary border-b-2 border-primary bg-primary/5'
              : 'text-base-content/40 hover:text-base-content/70'"
            @click="mode = 'register'; error = ''"
            data-testid="register-tab"
          >Create account</button>
        </div>

        <!-- Form -->
        <div class="p-7 space-y-5">
          <AlertMessage :message="error" @dismiss="error = ''" />
          <AlertMessage :message="successMsg" type="success" :dismissible="false" />

          <form @submit.prevent="handleSubmit" class="space-y-4">

            <div class="space-y-1.5">
              <label class="text-sm font-semibold text-base-content/70">Email</label>
              <input
                v-model="email"
                type="email"
                class="input input-bordered w-full"
                :class="fieldErrors.email ? 'input-error' : ''"
                placeholder="you@example.com"
                autocomplete="email"
                data-testid="email-input"
              />
              <p v-if="fieldErrors.email" class="text-xs text-error mt-0.5">{{ fieldErrors.email }}</p>
            </div>

            <div class="space-y-1.5">
              <label class="text-sm font-semibold text-base-content/70">Password</label>
              <input
                v-model="password"
                type="password"
                class="input input-bordered w-full"
                :class="fieldErrors.password ? 'input-error' : ''"
                placeholder="••••••••"
                autocomplete="current-password"
                data-testid="password-input"
              />
              <p v-if="fieldErrors.password" class="text-xs text-error mt-0.5">{{ fieldErrors.password }}</p>
            </div>

            <button
              type="submit"
              class="btn btn-primary w-full mt-1"
              :disabled="authStore.loading"
              :data-testid="mode === 'login' ? 'login-btn' : 'register-btn'"
            >
              <span v-if="authStore.loading" class="loading loading-spinner loading-xs"></span>
              {{ mode === 'login' ? 'Sign in' : 'Create Account' }}
            </button>
          </form>
        </div>
      </div>

      <!-- Footer note -->
      <p class="text-center text-xs text-base-content/30 mt-6">
        Premium wine management, simply yours.
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AlertMessage from '@/components/AlertMessage.vue'

const authStore = useAuthStore()
const router    = useRouter()
const route     = useRoute()

const mode        = ref('login')
const email       = ref('')
const password    = ref('')
const error       = ref('')
const successMsg  = ref('')
const fieldErrors = reactive({})

function validate() {
  Object.keys(fieldErrors).forEach(k => delete fieldErrors[k])
  if (!email.value.trim())            fieldErrors.email    = 'Email is required'
  else if (!email.value.includes('@')) fieldErrors.email    = 'Enter a valid email'
  if (!password.value)                 fieldErrors.password = 'Password is required'
  else if (password.value.length < 6)  fieldErrors.password = 'Password must be at least 6 characters'
  return Object.keys(fieldErrors).length === 0
}

async function handleSubmit() {
  if (!validate()) return
  error.value = successMsg.value = ''
  try {
    if (mode.value === 'login') {
      await authStore.login(email.value, password.value)
    } else {
      await authStore.register(email.value, password.value)
      successMsg.value = 'Account created! Redirecting…'
    }
    router.push(route.query.redirect || '/cellar')
  } catch (err) {
    error.value = err.message
  }
}
</script>
