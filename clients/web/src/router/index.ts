import { createRouter, createWebHistory } from 'vue-router'
import { authGuard } from '@auth0/auth0-vue'
import DashboardView from '../views/DashboardView.vue'
import UsersView from '../views/UsersView.vue'
import TasksView from '../views/TasksView.vue'
import CalendarView from '../views/CalendarView.vue'
import LoginView from '../views/LoginView.vue'
import RecurringView from '../views/RecurringView.vue'
import HistoryView from '../views/HistoryView.vue'

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
    path: '/recurring',
    name: 'Recurring',
    component: RecurringView,
    beforeEnter: authGuard,
    meta: { title: 'Récurrences', subtitle: 'Gérer les tâches répétitives et leur fréquence.' }
  },
  {
    path: '/history',
    name: 'History',
    component: HistoryView,
    beforeEnter: authGuard,
    meta: { title: 'Historique', subtitle: 'Tâches terminées et non faites.' }
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
