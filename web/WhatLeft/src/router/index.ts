import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '../views/DashboardView.vue'
import UsersView from '../views/UsersView.vue'
import SettingsView from '../views/SettingsView.vue'
import TasksView from '../views/TasksView.vue'
import LoginView from '../views/LoginView.vue'

const routes = [
  { path: '/', name: 'Dashboard', component: DashboardView },
  { path: '/tasks', name: 'Tasks', component: TasksView },
  { path: '/users', name: 'Users', component: UsersView },
  { path: '/settings', name: 'Settings', component: SettingsView },
  { path: '/login', name: 'Login', component: LoginView }
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
