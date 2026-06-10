import { test, expect } from '@playwright/test'

// Each test gets a fresh browser context → clean localStorage → app re-seeds mock data
const testEmail = () => `user${Date.now()}@test.com`
const testPassword = 'password123'

async function register(page, email = testEmail()) {
  await page.goto('/login')
  await page.click('[data-testid="register-tab"]')
  await page.fill('[data-testid="email-input"]', email)
  await page.fill('[data-testid="password-input"]', testPassword)
  await page.click('[data-testid="register-btn"]')
  await expect(page).toHaveURL('/cellar', { timeout: 5000 })
  return email
}

async function loginDemo(page) {
  await page.goto('/login')
  await page.fill('[data-testid="email-input"]', 'demo@winecellar.com')
  await page.fill('[data-testid="password-input"]', testPassword)
  await page.click('[data-testid="login-btn"]')
  await expect(page).toHaveURL('/cellar', { timeout: 5000 })
}

test('redirects unauthenticated user to login', async ({ page }) => {
  await page.goto('/cellar')
  await expect(page).toHaveURL(/\/login/)
})

test('register new user and land on cellar', async ({ page }) => {
  await register(page)
  await expect(page.locator('h2')).toContainText('My Cellar')
})

test('login with demo account', async ({ page }) => {
  await loginDemo(page)
  await expect(page.locator('h2')).toContainText('My Cellar')
})

test('login with wrong password shows error', async ({ page }) => {
  await page.goto('/login')
  await page.fill('[data-testid="email-input"]', 'demo@winecellar.com')
  await page.fill('[data-testid="password-input"]', 'wrongpassword')
  await page.click('[data-testid="login-btn"]')
  await expect(page.locator('.alert-danger')).toBeVisible()
})

test('browse wines page shows wine cards', async ({ page }) => {
  await loginDemo(page)
  await page.goto('/wines')
  await expect(page.locator('[data-testid="wine-card"]').first()).toBeVisible({ timeout: 5000 })
  const count = await page.locator('[data-testid="wine-card"]').count()
  expect(count).toBeGreaterThan(0)
})

test('search filters wine cards', async ({ page }) => {
  await loginDemo(page)
  await page.goto('/wines')
  await page.locator('[data-testid="wine-card"]').first().waitFor()
  const total = await page.locator('[data-testid="wine-card"]').count()
  await page.fill('input[placeholder*="Search"]', 'Margaux')
  await page.waitForTimeout(400)
  const filtered = await page.locator('[data-testid="wine-card"]').count()
  expect(filtered).toBeLessThan(total)
})

test('create a new wine', async ({ page }) => {
  await loginDemo(page)
  await page.goto('/wines')
  await page.click('[data-testid="create-wine-btn"]')
  await page.fill('[data-testid="wine-name-input"]', 'Test Pinot Noir')
  await page.fill('[data-testid="wine-domain-input"]', 'Test Domaine')
  await page.fill('[data-testid="wine-year-input"]', '2021')
  await page.fill('[data-testid="cepage-name-0"]', 'Pinot Noir')
  await page.fill('[data-testid="cepage-pct-0"]', '100')
  await page.click('[data-testid="wine-submit-btn"]')
  await expect(page.locator('.alert-success')).toBeVisible({ timeout: 3000 })
  await expect(page.locator('text=Test Pinot Noir')).toBeVisible({ timeout: 3000 })
})

test('add wine to cellar from wines page', async ({ page }) => {
  await register(page)
  await page.goto('/wines')
  await page.locator('[data-testid="wine-card"]').first().waitFor()
  await page.locator('[data-testid="add-to-cellar-btn"]').first().click()
  await expect(page.locator('[data-testid="remove-from-cellar-btn"]').first()).toBeVisible({ timeout: 3000 })
})

test('cellar shows pre-seeded wines for demo user', async ({ page }) => {
  await loginDemo(page)
  const items = page.locator('[data-testid="cellar-item"]')
  await items.first().waitFor({ timeout: 5000 })
  const count = await items.count()
  expect(count).toBeGreaterThan(0)
})

test('increment bottle count', async ({ page }) => {
  await loginDemo(page)
  const firstCount = page.locator('[data-testid="bottle-count"]').first()
  await firstCount.waitFor()
  const before = parseInt(await firstCount.textContent())
  await page.locator('[data-testid="increment-btn"]').first().click()
  await expect(firstCount).toContainText(String(before + 1), { timeout: 3000 })
})

test('decrement bottle count', async ({ page }) => {
  await loginDemo(page)
  const firstCount = page.locator('[data-testid="bottle-count"]').first()
  await firstCount.waitFor()
  const before = parseInt(await firstCount.textContent())
  await page.locator('[data-testid="decrement-btn"]').first().click()
  await expect(firstCount).toContainText(String(before - 1), { timeout: 3000 })
})

test('decrement to zero removes item from cellar', async ({ page }) => {
  await register(page)
  // Add a wine with 1 bottle
  await page.goto('/wines')
  await page.locator('[data-testid="add-to-cellar-btn"]').first().click()
  await page.goto('/cellar')
  const item = page.locator('[data-testid="cellar-item"]').first()
  await item.waitFor()
  await page.locator('[data-testid="decrement-btn"]').first().click()
  await page.waitForTimeout(500)
  const remaining = await page.locator('[data-testid="cellar-item"]').count()
  expect(remaining).toBe(0)
})

test('browse recipes page shows recipe cards', async ({ page }) => {
  await loginDemo(page)
  await page.goto('/recipes')
  await expect(page.locator('[data-testid="recipe-card"]').first()).toBeVisible({ timeout: 5000 })
  const count = await page.locator('[data-testid="recipe-card"]').count()
  expect(count).toBeGreaterThan(0)
})

test('create a new recipe', async ({ page }) => {
  await loginDemo(page)
  await page.goto('/recipes')
  await page.click('[data-testid="create-recipe-btn"]')
  await page.fill('[data-testid="recipe-name-input"]', 'Test Pasta Dish')
  await page.selectOption('[data-testid="recipe-type-input"]', 'Main')
  await page.fill('[data-testid="recipe-ingredients-input"]', '200g pasta\n100g cheese\n2 eggs')
  await page.fill('[data-testid="recipe-instructions-input"]', 'Cook pasta. Mix with cheese and eggs.')
  await page.click('[data-testid="recipe-submit-btn"]')
  await expect(page.locator('.alert-success')).toBeVisible({ timeout: 3000 })
  await expect(page.locator('text=Test Pasta Dish')).toBeVisible({ timeout: 3000 })
})

test('navigate to wine details', async ({ page }) => {
  await loginDemo(page)
  await page.goto('/wines')
  await page.locator('[data-testid="wine-card"]').first().waitFor()
  await page.locator('a:text("Details")').first().click()
  await expect(page).toHaveURL(/\/wines\/\d+/)
  await expect(page.locator('h2')).toBeVisible()
})

test('navigate to recipe details', async ({ page }) => {
  await loginDemo(page)
  await page.goto('/recipes')
  await page.locator('[data-testid="recipe-card"]').first().waitFor()
  await page.locator('a:text("View Recipe")').first().click()
  await expect(page).toHaveURL(/\/recipes\/\d+/)
})

test('logout redirects to login', async ({ page }) => {
  await loginDemo(page)
  await page.click('button:text("Logout")')
  await expect(page).toHaveURL(/\/login/)
})
