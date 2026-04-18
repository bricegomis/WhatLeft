<template>
  <v-app>
    <!-- App Bar (Header) - Rendu en premier pour la priorité -->
    <v-app-bar
      color="white"
      elevation="1"
      height="64"
    >
      <!-- Navigation Icon pour mobile -->
      <template v-slot:prepend>
        <v-app-bar-nav-icon
          v-if="isMobile"
          @click="drawer = !drawer"
        ></v-app-bar-nav-icon>
      </template>

      <!-- Titre de l'app -->
      <v-app-bar-title>WhatLeft</v-app-bar-title>

      <!-- Actions à droite -->
      <v-spacer></v-spacer>

      <!-- Barre de recherche -->
      <v-text-field
        v-model="search"
        placeholder="Rechercher..."
        prepend-inner-icon="mdi-magnify"
        variant="outlined"
        density="comfortable"
        style="max-width: 300px;"
        hide-details
        class="mr-4"
      ></v-text-field>

      <!-- Menu utilisateur -->
      <v-menu>
        <template v-slot:activator="{ props }">
          <v-btn
            icon
            v-bind="props"
            size="large"
          >
            <v-avatar size="40" color="primary">
              <span class="text-white font-weight-bold">A</span>
            </v-avatar>
          </v-btn>
        </template>

        <v-card width="280">
          <v-card-text class="pa-4">
            <div class="d-flex align-center mb-3">
              <v-avatar size="48" color="primary" class="me-3">
                <span class="text-white font-weight-bold">A</span>
              </v-avatar>
              <div>
                <div class="text-h6">Administrateur</div>
                <div class="text-body-2 text-medium-emphasis">whatleft@example.com</div>
              </div>
            </div>
            <v-divider class="my-3"></v-divider>
            <v-btn variant="text" block class="justify-start">
              <v-icon start>mdi-account</v-icon>
              Profil
            </v-btn>
            <v-btn variant="text" block class="justify-start">
              <v-icon start>mdi-cog</v-icon>
              Paramètres
            </v-btn>
            <v-btn variant="text" block class="justify-start">
              <v-icon start>mdi-logout</v-icon>
              Déconnexion
            </v-btn>
          </v-card-text>
        </v-card>
      </v-menu>
    </v-app-bar>

    <!-- Navigation Drawer (Sidebar) -->
    <v-navigation-drawer
      v-model="drawer"
      :permanent="!isMobile"
      :temporary="isMobile"
      width="280"
      color="grey-darken-4"
      dark
    >
      <!-- Logo/Brand -->
      <template v-slot:prepend>
        <v-toolbar
          color="grey-darken-4"
          flat
          height="64"
        >
          <v-toolbar-title class="text-h5 font-weight-bold">
            WhatLeft
          </v-toolbar-title>
        </v-toolbar>
      </template>

      <v-divider></v-divider>

      <!-- Navigation Menu -->
      <v-list nav dense>
        <v-list-item
          v-for="item in menuItems"
          :key="item.title"
          :to="item.path"
          :active="isActive(item.path)"
          color="primary"
        >
          <template v-slot:prepend>
            <v-icon>{{ item.icon }}</v-icon>
          </template>
          <v-list-item-title>{{ item.title }}</v-list-item-title>
        </v-list-item>
      </v-list>

      <template v-slot:append>
        <!-- Support Section -->
        <v-list nav dense>
          <v-subheader class="text-uppercase text-caption">Support</v-subheader>
          <v-list-item to="/login" color="primary">
            <template v-slot:prepend>
              <v-icon>mdi-login</v-icon>
            </template>
            <v-list-item-title>Se connecter</v-list-item-title>
          </v-list-item>
        </v-list>
      </template>
    </v-navigation-drawer>

    <!-- Main Content -->
    <v-main class="bg-grey-lighten-5">
      <v-container fluid class="pa-6">
        <slot />
      </v-container>
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'

// Reactive data
const drawer = ref(true)
const search = ref('')
const windowWidth = ref(window.innerWidth)

// Menu items
const menuItems = [
  { title: 'Tableau de bord', icon: 'mdi-view-dashboard', path: '/' },
  { title: 'Tâches', icon: 'mdi-check-circle-outline', path: '/tasks' },
  { title: 'Calendrier', icon: 'mdi-calendar', path: '/calendar' },
  { title: 'Utilisateurs', icon: 'mdi-account-group', path: '/users' },
  { title: 'Paramètres', icon: 'mdi-cog', path: '/settings' },
]

// Computed properties
const isMobile = computed(() => windowWidth.value < 960)

const route = useRoute()
const isActive = (path: string) => route.path === path

// Methods
const updateWindowWidth = () => {
  windowWidth.value = window.innerWidth
}

// Lifecycle hooks
onMounted(() => {
  window.addEventListener('resize', updateWindowWidth)
})

onUnmounted(() => {
  window.removeEventListener('resize', updateWindowWidth)
})
</script>

<style scoped>
/* Styles personnalisés si nécessaire */
</style>
