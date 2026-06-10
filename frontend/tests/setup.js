import { config } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, vi } from 'vitest'

// Provide a fresh pinia and stub localStorage before every test
beforeEach(() => {
  setActivePinia(createPinia())

  const store = {}
  vi.stubGlobal('localStorage', {
    getItem: vi.fn(key => store[key] ?? null),
    setItem: vi.fn((key, val) => { store[key] = val }),
    removeItem: vi.fn(key => { delete store[key] }),
    clear: vi.fn(() => { Object.keys(store).forEach(k => delete store[k]) })
  })
})

config.global.stubs = {
  'router-link': { template: '<a><slot /></a>' },
  'router-view': { template: '<div />' }
}
