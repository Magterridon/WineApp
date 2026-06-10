import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import LoginPage from '@/pages/LoginPage.vue'

const mockPush = vi.fn()

vi.mock('vue-router', async () => {
  const actual = await vi.importActual('vue-router')
  return {
    ...actual,
    useRouter: () => ({ push: mockPush }),
    useRoute: () => ({ query: {} })
  }
})

vi.mock('@/services/authService', () => ({
  authService: {
    login: vi.fn(),
    register: vi.fn()
  }
}))

import { authService } from '@/services/authService'

const fakeResult = { token: 'token', user: { id: 1, email: 'test@example.com' } }

function mountLoginPage() {
  return mount(LoginPage, {
    global: { plugins: [createPinia()] }
  })
}

describe('LoginPage', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    mockPush.mockClear()
  })

  it('renders email and password inputs', () => {
    const wrapper = mountLoginPage()
    expect(wrapper.find('[data-testid="email-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="password-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="login-btn"]').exists()).toBe(true)
  })

  it('shows register tab', () => {
    const wrapper = mountLoginPage()
    expect(wrapper.find('[data-testid="register-tab"]').exists()).toBe(true)
  })

  it('switches to register mode when register tab is clicked', async () => {
    const wrapper = mountLoginPage()
    await wrapper.find('[data-testid="register-tab"]').trigger('click')
    expect(wrapper.find('[data-testid="register-btn"]').exists()).toBe(true)
  })

  it('shows field error when email is empty', async () => {
    const wrapper = mountLoginPage()
    await wrapper.find('[data-testid="login-btn"]').trigger('click')
    expect(wrapper.text()).toContain('Email is required')
  })

  it('shows field error when password is empty', async () => {
    const wrapper = mountLoginPage()
    await wrapper.find('[data-testid="email-input"]').setValue('test@example.com')
    await wrapper.find('[data-testid="login-btn"]').trigger('click')
    expect(wrapper.text()).toContain('Password is required')
  })

  it('shows error when password too short', async () => {
    const wrapper = mountLoginPage()
    await wrapper.find('[data-testid="email-input"]').setValue('test@example.com')
    await wrapper.find('[data-testid="password-input"]').setValue('abc')
    await wrapper.find('[data-testid="login-btn"]').trigger('click')
    expect(wrapper.text()).toContain('at least 6 characters')
  })

  it('calls login service with correct credentials', async () => {
    authService.login.mockResolvedValue(fakeResult)
    const wrapper = mountLoginPage()
    await wrapper.find('[data-testid="email-input"]').setValue('test@example.com')
    await wrapper.find('[data-testid="password-input"]').setValue('password123')
    await wrapper.find('[data-testid="login-btn"]').trigger('click')
    await new Promise(r => setTimeout(r, 50))
    expect(authService.login).toHaveBeenCalledWith('test@example.com', 'password123')
  })

  it('redirects to cellar on successful login', async () => {
    authService.login.mockResolvedValue(fakeResult)
    const wrapper = mountLoginPage()
    await wrapper.find('[data-testid="email-input"]').setValue('test@example.com')
    await wrapper.find('[data-testid="password-input"]').setValue('password123')
    await wrapper.find('[data-testid="login-btn"]').trigger('click')
    await new Promise(r => setTimeout(r, 50))
    expect(mockPush).toHaveBeenCalledWith('/cellar')
  })

  it('shows error message on login failure', async () => {
    authService.login.mockRejectedValue(new Error('Invalid email or password'))
    const wrapper = mountLoginPage()
    await wrapper.find('[data-testid="email-input"]').setValue('bad@example.com')
    await wrapper.find('[data-testid="password-input"]').setValue('wrongpass')
    await wrapper.find('[data-testid="login-btn"]').trigger('click')
    await new Promise(r => setTimeout(r, 50))
    expect(wrapper.text()).toContain('Invalid email or password')
  })
})
