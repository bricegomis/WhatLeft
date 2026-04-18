import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '../views/DashboardView.vue'
import UsersView from '../views/UsersView.vue'
import SettingsView from '../views/SettingsView.vue'
import LoginView from '../views/LoginView.vue'

const routes = [
  { path: '/', name: 'Dashboard', component: DashboardView },
  { path: '/users', name: 'Users', component: UsersView },
  { path: '/settings', name: 'Settings', component: SettingsView },
  { path: '/login', name: 'Login', component: LoginView }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

router.beforeEach((to, _, next) => {
  document.title = to.name ? `WhatLeft • ${to.name}` : 'WhatLeft'
  next()
})

export default router
