import { createRouter, createWebHistory } from 'vue-router'
import { authGuard } from '@auth0/auth0-vue'
import DashboardView from '../views/DashboardView.vue'
import UsersView from '../views/UsersView.vue'
import TasksView from '../views/TasksView.vue'
import CalendarView from '../views/CalendarView.vue'
import LoginView from '../views/LoginView.vue'

const routes = [
  { path: '/login', name: 'Login', component: LoginView },
  {
    path: '/',
    name: 'Dashboard',
    component: DashboardView,
    beforeEnter: authGuard,
    meta: { title: 'Tableau de bord', subtitle: "Vue d'ensemble de l'activité et des performances." }
  },
  {
    path: '/tasks',
    name: 'Tasks',
    component: TasksView,
    beforeEnter: authGuard,
    meta: { title: 'Liste des tâches', subtitle: 'Suivre, planifier et terminer les tâches.' }
  },
  {
    path: '/calendar',
    name: 'Calendar',
    component: CalendarView,
    beforeEnter: authGuard,
    meta: { title: 'Calendrier', subtitle: 'Glissez les tâches à gauche sur le calendrier pour les planifier.' }
  },
  {
    path: '/users',
    name: 'Users',
    component: UsersView,
    beforeEnter: authGuard,
    meta: { title: 'Mon profil', subtitle: 'Informations de votre compte Auth0.' }
  },
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
