import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes = [
  { path: '/', redirect: '/cellar' },
  {
    path: '/login',
    name: 'login',
    component: () => import('@/pages/LoginPage.vue')
  },
  {
    path: '/cellar',
    name: 'cellar',
    component: () => import('@/pages/CellarPage.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/wines',
    name: 'wines',
    component: () => import('@/pages/WinesPage.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/wines/:id',
    name: 'wine-details',
    component: () => import('@/pages/WineDetailsPage.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/recipes',
    name: 'recipes',
    component: () => import('@/pages/RecipesPage.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/recipes/:id',
    name: 'recipe-details',
    component: () => import('@/pages/RecipeDetailsPage.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/admin/wines',
    name: 'admin-wines',
    component: () => import('@/pages/AdminWinesPage.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to) => {
  const authStore = useAuthStore()
  authStore.init()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    return { name: 'cellar' }
  }

  if (to.name === 'login' && authStore.isAuthenticated) {
    return { name: 'cellar' }
  }
})

export default router
