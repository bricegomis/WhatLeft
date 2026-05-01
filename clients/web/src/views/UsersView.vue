<template>
  <AdminLayout>
    <v-row>
      <v-col cols="12" md="4">
        <v-card class="pa-4 text-center">
          <v-avatar size="96" class="mb-4">
            <v-img :src="avatarUrl" :alt="user?.name" />
          </v-avatar>
          <div class="text-h6 font-weight-bold">{{ user?.name }}</div>
          <div class="text-body-2 text-medium-emphasis mb-4">{{ user?.email }}</div>
          <v-chip v-if="user?.email_verified" color="success" size="small" prepend-icon="mdi-check-circle">
            Email vérifié
          </v-chip>
          <v-chip v-else color="warning" size="small" prepend-icon="mdi-alert-circle">
            Email non vérifié
          </v-chip>
        </v-card>
      </v-col>

      <v-col cols="12" md="8">
        <v-card>
          <v-card-title class="text-h6">Détails du compte</v-card-title>
          <v-list lines="two">
            <v-list-item
              v-for="field in profileFields"
              :key="field.label"
              :subtitle="field.label"
              :title="field.value || '—'"
            >
              <template #prepend>
                <v-icon :icon="field.icon" class="mr-2" />
              </template>
            </v-list-item>
          </v-list>
        </v-card>

        <v-card class="mt-4">
          <v-card-title class="text-h6">Données brutes Auth0</v-card-title>
          <v-card-text>
            <pre class="text-body-2" style="overflow-x: auto;">{{ JSON.stringify(user, null, 2) }}</pre>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </AdminLayout>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuth0 } from '@auth0/auth0-vue'
import { gravatarUrl } from '../composables/useGravatar'
import AdminLayout from '../layouts/AdminLayout.vue'

const { user } = useAuth0()

const avatarUrl = computed(() => gravatarUrl(user.value?.email, 192))

const profileFields = computed(() => [
  { label: 'Identifiant', value: user.value?.sub, icon: 'mdi-identifier' },
  { label: 'Nom complet', value: user.value?.name, icon: 'mdi-account' },
  { label: 'Surnom', value: user.value?.nickname, icon: 'mdi-at' },
  { label: 'Email', value: user.value?.email, icon: 'mdi-email' },
  { label: 'Fournisseur', value: user.value?.sub?.split('|')[0], icon: 'mdi-shield-account' },
  { label: 'Dernière mise à jour', value: user.value?.updated_at ? new Date(user.value.updated_at).toLocaleString('fr-FR') : null, icon: 'mdi-clock' },
])
</script>
