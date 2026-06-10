import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '@/stores/auth'

vi.mock('@/services/authService', () => ({
  authService: {
    login: vi.fn(),
    register: vi.fn()
  }
}))

import { authService } from '@/services/authService'

const fakeResult = { token: 'fake-token', user: { id: 1, email: 'test@example.com' } }

describe('auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('starts unauthenticated', () => {
    const store = useAuthStore()
    expect(store.isAuthenticated).toBe(false)
    expect(store.user).toBeNull()
  })

  it('login sets token and user', async () => {
    authService.login.mockResolvedValue(fakeResult)
    const store = useAuthStore()
    await store.login('test@example.com', 'password')
    expect(store.isAuthenticated).toBe(true)
    expect(store.user.email).toBe('test@example.com')
    expect(store.token).toBe('fake-token')
  })

  it('login stores credentials in localStorage', async () => {
    authService.login.mockResolvedValue(fakeResult)
    const store = useAuthStore()
    await store.login('test@example.com', 'password')
    expect(localStorage.setItem).toHaveBeenCalledWith('wca_token', 'fake-token')
    expect(localStorage.setItem).toHaveBeenCalledWith('wca_user', JSON.stringify(fakeResult.user))
  })

  it('login sets error on failure', async () => {
    authService.login.mockRejectedValue(new Error('Invalid credentials'))
    const store = useAuthStore()
    await expect(store.login('bad@example.com', 'wrong')).rejects.toThrow('Invalid credentials')
    expect(store.error).toBe('Invalid credentials')
    expect(store.isAuthenticated).toBe(false)
  })

  it('register sets token and user', async () => {
    authService.register.mockResolvedValue(fakeResult)
    const store = useAuthStore()
    await store.register('new@example.com', 'password123')
    expect(store.isAuthenticated).toBe(true)
  })

  it('logout clears auth state', async () => {
    authService.login.mockResolvedValue(fakeResult)
    const store = useAuthStore()
    await store.login('test@example.com', 'password')
    store.logout()
    expect(store.isAuthenticated).toBe(false)
    expect(store.user).toBeNull()
    expect(store.token).toBeNull()
  })

  it('logout removes from localStorage', async () => {
    authService.login.mockResolvedValue(fakeResult)
    const store = useAuthStore()
    await store.login('test@example.com', 'password')
    store.logout()
    expect(localStorage.removeItem).toHaveBeenCalledWith('wca_token')
    expect(localStorage.removeItem).toHaveBeenCalledWith('wca_user')
  })

  it('init restores session from localStorage', () => {
    localStorage.getItem.mockImplementation(key => {
      if (key === 'wca_token') return 'stored-token'
      if (key === 'wca_user') return JSON.stringify({ id: 1, email: 'test@example.com' })
      return null
    })
    const store = useAuthStore()
    store.init()
    expect(store.isAuthenticated).toBe(true)
    expect(store.user.email).toBe('test@example.com')
  })

  it('init does nothing when no stored session', () => {
    const store = useAuthStore()
    store.init()
    expect(store.isAuthenticated).toBe(false)
  })
})
