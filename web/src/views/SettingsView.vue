<template>
  <AdminLayout>
    <!-- Page Header -->
    <v-row class="mb-6">
      <v-col cols="12">
        <div>
          <h1 class="text-h4 font-weight-bold mb-2">Paramètres</h1>
          <p class="text-body-1 text-medium-emphasis mb-0">
            Configurer le tableau de bord et les préférences système.
          </p>
        </div>
      </v-col>
    </v-row>

    <!-- General Settings -->
    <v-card class="mb-6">
      <v-card-title class="text-h6">Préférences générales</v-card-title>
      <v-card-text>
        <v-text-field
          v-model="siteName"
          label="Nom du site"
          class="mb-4"
        />

        <v-select
          v-model="timezone"
          label="Fuseau horaire"
          :items="timezones"
          class="mb-6"
        />

        <v-btn color="primary" @click="saveSettings">
          Sauvegarder
        </v-btn>
      </v-card-text>
    </v-card>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import AdminLayout from '../layouts/AdminLayout.vue'

const siteName = ref('WhatLeft Admin')
const timezone = ref('Europe/Paris')
const timezones = [
  'Europe/Paris',
  'Europe/London',
  'UTC'
]

const saveSettings = () => {
  console.log('Paramètres sauvegardés:', { siteName: siteName.value, timezone: timezone.value })
}
</script>
