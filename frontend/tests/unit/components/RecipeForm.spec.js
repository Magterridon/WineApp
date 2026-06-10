import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import RecipeForm from '@/components/RecipeForm.vue'

vi.mock('@/services/wineService', () => ({
  wineService: {
    getAll: vi.fn().mockResolvedValue([
      { id: 1, name: 'Château Margaux', domain: 'Château Margaux', year: 2018, cepages: [] },
      { id: 2, name: 'Gevrey-Chambertin', domain: 'Rossignol-Trapet', year: 2019, cepages: [] }
    ])
  }
}))

function mountForm(props = {}) {
  return mount(RecipeForm, { props })
}

describe('RecipeForm', () => {
  it('renders all required fields', () => {
    const wrapper = mountForm()
    expect(wrapper.find('[data-testid="recipe-name-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="recipe-type-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="recipe-ingredients-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="recipe-instructions-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="recipe-submit-btn"]').exists()).toBe(true)
  })

  it('shows validation error when name is empty', async () => {
    const wrapper = mountForm()
    await wrapper.find('[data-testid="recipe-submit-btn"]').trigger('click')
    expect(wrapper.text()).toContain('Name is required')
  })

  it('shows validation error when ingredients are empty', async () => {
    const wrapper = mountForm()
    await wrapper.find('[data-testid="recipe-name-input"]').setValue('Boeuf Bourguignon')
    await wrapper.find('[data-testid="recipe-instructions-input"]').setValue('Some instructions')
    await wrapper.find('[data-testid="recipe-submit-btn"]').trigger('click')
    expect(wrapper.text()).toContain('Ingredients are required')
  })

  it('emits submit with parsed ingredients array on valid form', async () => {
    const wrapper = mountForm()
    await wrapper.find('[data-testid="recipe-name-input"]').setValue('Test Recipe')
    await wrapper.find('[data-testid="recipe-ingredients-input"]').setValue('500g beef\n2 carrots\n1 onion')
    await wrapper.find('[data-testid="recipe-instructions-input"]').setValue('Cook everything.')
    await wrapper.find('[data-testid="recipe-submit-btn"]').trigger('click')
    expect(wrapper.emitted('submit')).toBeTruthy()
    const payload = wrapper.emitted('submit')[0][0]
    expect(payload.name).toBe('Test Recipe')
    expect(payload.ingredients).toEqual(['500g beef', '2 carrots', '1 onion'])
  })

  it('emits cancel when cancel button is clicked', async () => {
    const wrapper = mountForm()
    await wrapper.find('[data-testid="recipe-cancel-btn"]').trigger('click')
    expect(wrapper.emitted('cancel')).toBeTruthy()
  })

  it('pre-fills fields when initialData is provided', () => {
    const initialData = {
      name: 'Boeuf Bourguignon',
      recipeType: 'Main',
      description: 'Classic dish',
      imageUrl: '',
      ingredients: ['500g beef', '2 carrots'],
      instructions: 'Cook for 3 hours.',
      pairings: []
    }
    const wrapper = mountForm({ initialData })
    expect(wrapper.find('[data-testid="recipe-name-input"]').element.value).toBe('Boeuf Bourguignon')
    expect(wrapper.find('[data-testid="recipe-submit-btn"]').text()).toContain('Save Changes')
  })
})
