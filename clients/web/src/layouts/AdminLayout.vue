<template>
  <v-layout>
    <!-- APP BAR -->
    <v-app-bar color="white" elevation="1" height="64">
      <v-app-bar-nav-icon
        variant="text"
        @click.stop="drawer = !drawer"
      />

      <div class="d-flex flex-column justify-center" style="min-width: 0;">
        <v-toolbar-title class="text-subtitle-1 text-sm-h6 font-weight-bold mb-0" style="line-height: 1.2;">
          {{ pageTitle }}
        </v-toolbar-title>
        <div
          v-if="pageSubtitle && $vuetify.display.smAndUp"
          class="text-caption text-medium-emphasis text-truncate"
          style="line-height: 1.2;"
        >
          {{ pageSubtitle }}
        </div>
      </div>

      <v-spacer />

      <!-- Slot pour les actions de page (boutons spécifiques) -->
      <slot name="actions" />

      <template v-if="$vuetify.display.mdAndUp">
        <v-text-field
          v-model="search"
          placeholder="Rechercher..."
          prepend-inner-icon="mdi-magnify"
          variant="outlined"
          density="comfortable"
          hide-details
          style="max-width: 260px"
          class="mx-4"
        />
      </template>

      <!-- Menu utilisateur -->
      <v-menu>
        <template #activator="{ props }">
          <v-btn icon v-bind="props">
            <v-avatar size="40">
              <v-img :src="avatarUrl" :alt="user?.name" />
            </v-avatar>
          </v-btn>
        </template>

        <v-card width="260">
          <v-card-text>
            <div class="d-flex align-center mb-3">
              <v-avatar size="48" class="mr-3">
                <v-img :src="avatarUrl" :alt="user?.name" />
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
      :location="$vuetify.display.mobile ? 'top' : 'left'"
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
    </v-navigation-drawer>

    <!-- MAIN CONTENT -->
    <v-main class="bg-grey-lighten-5">
      <v-container fluid :class="$vuetify.display.mobile ? 'pa-3' : 'pa-6'">
        <slot />
      </v-container>
    </v-main>
  </v-layout>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useDisplay } from 'vuetify'
import { useRoute } from 'vue-router'
import { useAuth0 } from '@auth0/auth0-vue'
import { gravatarUrl } from '../composables/useGravatar'

const { user, logout } = useAuth0()
const { mobile } = useDisplay()
// Drawer fermé par défaut sur mobile, ouvert sur desktop
const drawer = ref(!mobile.value)
const search = ref('')

const avatarUrl = computed(() => gravatarUrl(user.value?.email, 80))

const handleLogout = () =>
  logout({ logoutParams: { returnTo: window.location.origin } });
const menuItems = [
  { title: 'Tableau de bord', icon: 'mdi-view-dashboard', path: '/' },
  { title: 'Tâches', icon: 'mdi-check-circle-outline', path: '/tasks' },
  { title: 'Calendrier', icon: 'mdi-calendar', path: '/calendar' },
]

const route = useRoute()
const pageTitle = computed(() => (route.meta.title as string) ?? 'WhatLeft')
const pageSubtitle = computed(() => (route.meta.subtitle as string) ?? '')
const isActive = (path: string) => route.path === path
</script>

<style scoped>
</style>
