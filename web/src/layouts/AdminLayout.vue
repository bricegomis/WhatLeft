<template>
  <v-layout>
    <!-- APP BAR -->
    <v-app-bar color="white" elevation="1" height="64">
      <v-app-bar-nav-icon
        variant="text"
        @click.stop="drawer = !drawer"
      />

      <v-toolbar-title>WhatLeft</v-toolbar-title>

      <template v-if="$vuetify.display.mdAndUp">
        <v-text-field
          v-model="search"
          placeholder="Rechercher..."
          prepend-inner-icon="mdi-magnify"
          variant="outlined"
          density="comfortable"
          hide-details
          style="max-width: 260px"
          class="mr-4"
        />
      </template>

      <!-- Menu utilisateur -->
      <v-menu>
        <template #activator="{ props }">
          <v-btn icon v-bind="props">
            <v-avatar size="40" color="primary">
              <span class="text-white font-weight-bold">A</span>
            </v-avatar>
          </v-btn>
        </template>

        <v-card width="260">
          <v-card-text>
            <div class="d-flex align-center mb-3">
              <v-avatar size="48" color="primary" class="mr-3">
                <span class="text-white font-weight-bold">A</span>
              </v-avatar>
              <div>
                <div class="text-h6">Administrateur</div>
                <div class="text-body-2 text-medium-emphasis">
                  whatleft@example.com
                </div>
              </div>
            </div>

            <v-divider class="my-3" />

            <v-btn variant="text" block class="justify-start">
              <v-icon start>mdi-account</v-icon> Profil
            </v-btn>

            <v-btn variant="text" block class="justify-start">
              <v-icon start>mdi-cog</v-icon> Paramètres
            </v-btn>

            <v-btn variant="text" block class="justify-start">
              <v-icon start>mdi-logout</v-icon> Déconnexion
            </v-btn>
          </v-card-text>
        </v-card>
      </v-menu>
    </v-app-bar>

    <!-- DRAWER -->
    <v-navigation-drawer
      v-model="drawer"
      :location="$vuetify.display.mobile ? 'bottom' : 'left'"
      :temporary="$vuetify.display.mobile"
      :permanent="!$vuetify.display.mobile"
      width="260"
      color="grey-darken-4"
      dark
    >
      <v-list nav dense>
        <v-list-item
          v-for="item in menuItems"
          :key="item.title"
          :to="item.path"
          :active="isActive(item.path)"
          color="primary"
        >
          <template #prepend>
            <v-icon>{{ item.icon }}</v-icon>
          </template>
          <v-list-item-title>{{ item.title }}</v-list-item-title>
        </v-list-item>
      </v-list>

      <template #append>
        <v-divider />
        <v-list nav dense>
          <v-subheader class="text-caption">Support</v-subheader>
          <v-list-item to="/login">
            <template #prepend>
              <v-icon>mdi-login</v-icon>
            </template>
            <v-list-item-title>Se connecter</v-list-item-title>
          </v-list-item>
        </v-list>
      </template>
    </v-navigation-drawer>

    <!-- MAIN CONTENT -->
    <v-main class="bg-grey-lighten-5">
      <v-container fluid class="pa-6">
        <slot />
      </v-container>
    </v-main>
  </v-layout>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRoute } from 'vue-router'

const drawer = ref(true)
const search = ref('')

const menuItems = [
  { title: 'Tableau de bord', icon: 'mdi-view-dashboard', path: '/' },
  { title: 'Tâches', icon: 'mdi-check-circle-outline', path: '/tasks' },
  { title: 'Calendrier', icon: 'mdi-calendar', path: '/calendar' },
  { title: 'Utilisateurs', icon: 'mdi-account-group', path: '/users' },
  { title: 'Paramètres', icon: 'mdi-cog', path: '/settings' },
]

const route = useRoute()
const isActive = (path: string) => route.path === path
</script>

<style scoped>
</style>
