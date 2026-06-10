import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import WineForm from '@/components/WineForm.vue'

function mountForm(props = {}) {
  return mount(WineForm, { props })
}

describe('WineForm', () => {
  it('renders all required fields', () => {
    const wrapper = mountForm()
    expect(wrapper.find('[data-testid="wine-name-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="wine-domain-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="wine-year-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="wine-submit-btn"]').exists()).toBe(true)
  })

  it('shows validation error when name is empty', async () => {
    const wrapper = mountForm()
    await wrapper.find('[data-testid="wine-submit-btn"]').trigger('click')
    expect(wrapper.text()).toContain('Name is required')
  })

  it('shows validation error when domain is empty', async () => {
    const wrapper = mountForm()
    await wrapper.find('[data-testid="wine-name-input"]').setValue('Test Wine')
    await wrapper.find('[data-testid="wine-submit-btn"]').trigger('click')
    expect(wrapper.text()).toContain('Domain is required')
  })

  it('emits submit with form data when valid', async () => {
    const wrapper = mountForm()
    await wrapper.find('[data-testid="wine-name-input"]').setValue('Test Wine')
    await wrapper.find('[data-testid="wine-domain-input"]').setValue('Test Domain')
    await wrapper.find('[data-testid="wine-year-input"]').setValue('2020')
    await wrapper.find('[data-testid="wine-submit-btn"]').trigger('click')
    expect(wrapper.emitted('submit')).toBeTruthy()
    const payload = wrapper.emitted('submit')[0][0]
    expect(payload.name).toBe('Test Wine')
    expect(payload.domain).toBe('Test Domain')
  })

  it('emits cancel when cancel button is clicked', async () => {
    const wrapper = mountForm()
    await wrapper.find('[data-testid="wine-cancel-btn"]').trigger('click')
    expect(wrapper.emitted('cancel')).toBeTruthy()
  })

  it('can add and remove cepages', async () => {
    const wrapper = mountForm()
    expect(wrapper.findAll('[data-testid^="cepage-name-"]')).toHaveLength(1)
    await wrapper.find('[data-testid="add-cepage-btn"]').trigger('click')
    expect(wrapper.findAll('[data-testid^="cepage-name-"]')).toHaveLength(2)
  })

  it('pre-fills fields when initialData is provided', () => {
    const initialData = {
      name: 'Château Margaux',
      domain: 'Château Margaux',
      year: 2018,
      rank: 5,
      description: 'A great wine',
      imageUrl: '',
      drinkFromYear: 2026,
      drinkToYear: 2050,
      cepages: [{ name: 'Cabernet Sauvignon', percentage: 75 }]
    }
    const wrapper = mountForm({ initialData })
    expect(wrapper.find('[data-testid="wine-name-input"]').element.value).toBe('Château Margaux')
    expect(wrapper.find('[data-testid="wine-domain-input"]').element.value).toBe('Château Margaux')
    expect(wrapper.find('[data-testid="wine-submit-btn"]').text()).toContain('Save Changes')
  })
})
