import { createRouter, createWebHistory } from 'vue-router'
import { authGuard } from '@auth0/auth0-vue'
import DashboardView from '../views/DashboardView.vue'
import UsersView from '../views/UsersView.vue'
import SettingsView from '../views/SettingsView.vue'
import TasksView from '../views/TasksView.vue'
import CalendarView from '../views/CalendarView.vue'
import LoginView from '../views/LoginView.vue'

const routes = [
  { path: '/login', name: 'Login', component: LoginView },
  { path: '/', name: 'Dashboard', component: DashboardView, beforeEnter: authGuard },
  { path: '/tasks', name: 'Tasks', component: TasksView, beforeEnter: authGuard },
  { path: '/calendar', name: 'Calendar', component: CalendarView, beforeEnter: authGuard },
  { path: '/users', name: 'Users', component: UsersView, beforeEnter: authGuard },
  { path: '/settings', name: 'Settings', component: SettingsView, beforeEnter: authGuard },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

router.beforeEach((to, _, next) => {
  const routeName = to.name ? String(to.name) : 'WhatLeft'
  document.title = routeName === 'WhatLeft' ? 'WhatLeft' : `WhatLeft • ${routeName}`
  next()
})

export default router
