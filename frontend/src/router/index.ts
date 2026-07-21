import { createRouter, createWebHistory } from 'vue-router'

import { useAuthStore } from '@/stores/auth'
// Eagerly loaded: the entry view for a first, unauthenticated visit.
import LoginView from '@/views/LoginView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { requiresAuth: false, guestOnly: true },
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/views/RegisterView.vue'),
      meta: { requiresAuth: false, guestOnly: true },
    },
    {
      path: '/login/2fa',
      name: 'login-2fa',
      component: () => import('@/views/TwoFactorChallengeView.vue'),
      meta: { requiresAuth: false, guestOnly: true },
    },
    {
      path: '/',
      name: 'dashboard',
      component: () => import('@/views/DashboardView.vue'),
      meta: { requiresAuth: true, title: 'Resumen' },
    },
    {
      path: '/expenses',
      name: 'expenses',
      component: () => import('@/views/ExpensesView.vue'),
      meta: { requiresAuth: true, title: 'Gastos' },
    },
    {
      path: '/budgets',
      name: 'budgets',
      component: () => import('@/views/BudgetsView.vue'),
      meta: { requiresAuth: true, title: 'Presupuestos' },
    },
    {
      path: '/recurring',
      name: 'recurring',
      component: () => import('@/views/RecurringExpensesView.vue'),
      meta: { requiresAuth: true, title: 'Gastos recurrentes' },
    },
    {
      path: '/import',
      name: 'import',
      component: () => import('@/views/ImportView.vue'),
      meta: { requiresAuth: true, title: 'Importar gastos' },
    },
    {
      path: '/categories',
      name: 'categories',
      component: () => import('@/views/CategoriesView.vue'),
      meta: { requiresAuth: true, title: 'Categorías' },
    },
    {
      path: '/security',
      name: 'security',
      component: () => import('@/views/SecurityView.vue'),
      meta: { requiresAuth: true, title: 'Seguridad' },
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue'),
    },
  ],
})

router.beforeEach((to) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.meta.guestOnly && authStore.isAuthenticated) {
    return { name: 'dashboard' }
  }

  return true
})

export default router
