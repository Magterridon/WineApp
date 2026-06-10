import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useCellarStore } from '@/stores/cellar'

const mockWine1 = { id: 1, name: 'Château Margaux', domain: 'Château Margaux', year: 2018 }
const mockWine2 = { id: 2, name: 'Gevrey-Chambertin', domain: 'Rossignol-Trapet', year: 2019 }

vi.mock('@/services/cellarService', () => ({
  cellarService: {
    getCellar: vi.fn(),
    addWine: vi.fn(),
    increment: vi.fn(),
    decrement: vi.fn(),
    removeWine: vi.fn()
  }
}))

import { cellarService } from '@/services/cellarService'

describe('cellar store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('starts with empty items', () => {
    const store = useCellarStore()
    expect(store.items).toEqual([])
  })

  it('fetchCellar loads items', async () => {
    const items = [
      { wineId: 1, bottleCount: 3, wine: mockWine1 },
      { wineId: 2, bottleCount: 6, wine: mockWine2 }
    ]
    cellarService.getCellar.mockResolvedValue(items)
    const store = useCellarStore()
    await store.fetchCellar()
    expect(store.items).toEqual(items)
    expect(store.loading).toBe(false)
  })

  it('isInCellar returns true for present wines', async () => {
    cellarService.getCellar.mockResolvedValue([{ wineId: 1, bottleCount: 2, wine: mockWine1 }])
    const store = useCellarStore()
    await store.fetchCellar()
    expect(store.isInCellar(1)).toBe(true)
    expect(store.isInCellar(99)).toBe(false)
  })

  it('addWine appends new item', async () => {
    cellarService.getCellar.mockResolvedValue([])
    cellarService.addWine.mockResolvedValue({ wineId: 1, bottleCount: 1, wine: mockWine1 })
    const store = useCellarStore()
    await store.fetchCellar()
    await store.addWine(1)
    expect(store.items).toHaveLength(1)
    expect(store.items[0].bottleCount).toBe(1)
  })

  it('addWine updates existing item', async () => {
    const existing = { wineId: 1, bottleCount: 1, wine: mockWine1 }
    cellarService.getCellar.mockResolvedValue([existing])
    cellarService.addWine.mockResolvedValue({ wineId: 1, bottleCount: 2, wine: mockWine1 })
    const store = useCellarStore()
    await store.fetchCellar()
    await store.addWine(1)
    expect(store.items[0].bottleCount).toBe(2)
  })

  it('increment increases bottle count', async () => {
    cellarService.getCellar.mockResolvedValue([{ wineId: 1, bottleCount: 2, wine: mockWine1 }])
    cellarService.increment.mockResolvedValue({ wineId: 1, bottleCount: 3, wine: mockWine1 })
    const store = useCellarStore()
    await store.fetchCellar()
    await store.increment(1)
    expect(store.items[0].bottleCount).toBe(3)
  })

  it('decrement decreases bottle count', async () => {
    cellarService.getCellar.mockResolvedValue([{ wineId: 1, bottleCount: 3, wine: mockWine1 }])
    cellarService.decrement.mockResolvedValue({ wineId: 1, bottleCount: 2, wine: mockWine1 })
    const store = useCellarStore()
    await store.fetchCellar()
    await store.decrement(1)
    expect(store.items[0].bottleCount).toBe(2)
  })

  it('decrement removes item when count reaches zero', async () => {
    cellarService.getCellar.mockResolvedValue([{ wineId: 1, bottleCount: 1, wine: mockWine1 }])
    cellarService.decrement.mockResolvedValue(null)
    const store = useCellarStore()
    await store.fetchCellar()
    await store.decrement(1)
    expect(store.items).toHaveLength(0)
  })

  it('removeWine removes the item', async () => {
    cellarService.getCellar.mockResolvedValue([
      { wineId: 1, bottleCount: 2, wine: mockWine1 },
      { wineId: 2, bottleCount: 4, wine: mockWine2 }
    ])
    cellarService.removeWine.mockResolvedValue()
    const store = useCellarStore()
    await store.fetchCellar()
    await store.removeWine(1)
    expect(store.items).toHaveLength(1)
    expect(store.items[0].wineId).toBe(2)
  })
})
