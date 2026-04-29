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
              <v-img v-if="user?.picture" :src="user.picture" :alt="user?.name" />
              <span v-else class="text-white font-weight-bold">{{ userInitial }}</span>
            </v-avatar>
          </v-btn>
        </template>

        <v-card width="260">
          <v-card-text>
            <div class="d-flex align-center mb-3">
              <v-avatar size="48" color="primary" class="mr-3">
                <v-img v-if="user?.picture" :src="user.picture" :alt="user?.name" />
                <span v-else class="text-white font-weight-bold">{{ userInitial }}</span>
              </v-avatar>
              <div>
                <div class="text-h6">{{ user?.name }}</div>
                <div class="text-body-2 text-medium-emphasis">{{ user?.email }}</div>
              </div>
            </div>

            <v-divider class="my-3" />

            <v-btn variant="text" block class="justify-start" to="/users">
              <v-icon start>mdi-account</v-icon> Profil
            </v-btn>

            <v-btn variant="text" block class="justify-start" to="/settings">
              <v-icon start>mdi-cog</v-icon> Paramètres
            </v-btn>

            <v-btn variant="text" block class="justify-start" @click="handleLogout">
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
          <v-list-item @click="handleLogout">
            <template #prepend>
              <v-icon>mdi-logout</v-icon>
            </template>
            <v-list-item-title>Déconnexion</v-list-item-title>
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
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuth0 } from '@auth0/auth0-vue'

const { user, logout } = useAuth0()
const drawer = ref(true)
const search = ref('')

const userInitial = computed(() => user.value?.name?.charAt(0).toUpperCase() ?? '?')

const handleLogout = () =>
  logout({ logoutParams: { returnTo: window.location.origin + '/login' } });
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
