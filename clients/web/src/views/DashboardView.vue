<template>
  <AdminLayout>
    <template #actions>
      <v-btn
        color="primary"
        prepend-icon="mdi-plus"
        class="me-2"
      >
        <span class="d-none d-sm-inline">Nouvelle action</span>
      </v-btn>
    </template>

    <div>
      <!-- Stats Cards -->
      <v-row class="mb-6">
        <v-col cols="12" md="4">
          <v-card
            class="pa-6"
            elevation="2"
          >
            <div class="d-flex align-center">
              <v-avatar
                color="primary"
                size="48"
                class="me-4"
              >
                <v-icon color="white">mdi-currency-eur</v-icon>
              </v-avatar>
              <div>
                <div class="text-h5 font-weight-bold">24 580 €</div>
                <div class="text-body-2 text-medium-emphasis">
                  +12 % par rapport à la semaine dernière
                </div>
              </div>
            </div>
          </v-card>
        </v-col>

        <v-col cols="12" md="4">
          <v-card
            class="pa-6"
            elevation="2"
          >
            <div class="d-flex align-center">
              <v-avatar
                color="success"
                size="48"
                class="me-4"
              >
                <v-icon color="white">mdi-account-plus</v-icon>
              </v-avatar>
              <div>
                <div class="text-h5 font-weight-bold">1 240</div>
                <div class="text-body-2 text-medium-emphasis">
                  Nouveaux utilisateurs cette semaine
                </div>
              </div>
            </div>
          </v-card>
        </v-col>

        <v-col cols="12" md="4">
          <v-card
            class="pa-6"
            elevation="2"
          >
            <div class="d-flex align-center">
              <v-avatar
                color="warning"
                size="48"
                class="me-4"
              >
                <v-icon color="white">mdi-ticket-outline</v-icon>
              </v-avatar>
              <div>
                <div class="text-h5 font-weight-bold">18</div>
                <div class="text-body-2 text-medium-emphasis">
                  Tickets ouverts
                </div>
              </div>
            </div>
          </v-card>
        </v-col>
      </v-row>

      <!-- Recent Activity Table -->
      <v-row>
        <v-col cols="12">
          <v-card
            elevation="2"
          >
            <v-card-title class="d-flex align-center">
              <v-icon start>mdi-history</v-icon>
              Activité récente
            </v-card-title>

            <v-data-table
              :headers="activityHeaders"
              :items="recentActivities"
              :items-per-page="5"
              density="comfortable"
              class="elevation-0"
            >
              <template v-slot:item.status="{ item }">
                <v-chip
                  :color="getStatusColor(item.status)"
                  size="small"
                  variant="flat"
                >
                  {{ item.status }}
                </v-chip>
              </template>

              <template v-slot:item.timestamp="{ item }">
                <span class="text-body-2">{{ formatDate(item.timestamp) }}</span>
              </template>
            </v-data-table>
          </v-card>
        </v-col>
      </v-row>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import AdminLayout from '../layouts/AdminLayout.vue'

// Activity table headers
const activityHeaders = [
  { title: 'Action', key: 'action', width: '40%' },
  { title: 'Utilisateur', key: 'user', width: '30%' },
  { title: 'Statut', key: 'status', width: '15%' },
  { title: 'Date', key: 'timestamp', width: '15%' }
]

// Recent activities data
const recentActivities = ref([
  {
    action: 'Mise à jour du financement',
    user: 'Claire Martin',
    status: 'Complété',
    timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000) // 2 hours ago
  },
  {
    action: 'Ajout d\'un nouveau projet',
    user: 'Romain Lacroix',
    status: 'En attente',
    timestamp: new Date(Date.now() - 4 * 60 * 60 * 1000) // 4 hours ago
  },
  {
    action: 'Connexion API',
    user: 'Sophie Dubois',
    status: 'Complété',
    timestamp: new Date(Date.now() - 6 * 60 * 60 * 1000) // 6 hours ago
  },
  {
    action: 'Modification des paramètres',
    user: 'Lucas Moreau',
    status: 'En cours',
    timestamp: new Date(Date.now() - 8 * 60 * 60 * 1000) // 8 hours ago
  },
  {
    action: 'Rapport généré',
    user: 'Emma Petit',
    status: 'Complété',
    timestamp: new Date(Date.now() - 12 * 60 * 60 * 1000) // 12 hours ago
  }
])

// Helper functions
const getStatusColor = (status: string) => {
  switch (status) {
    case 'Complété':
      return 'success'
    case 'En attente':
      return 'warning'
    case 'En cours':
      return 'info'
    case 'Erreur':
      return 'error'
    default:
      return 'grey'
  }
}

const formatDate = (date: Date) => {
  return new Intl.DateTimeFormat('fr-FR', {
    hour: '2-digit',
    minute: '2-digit',
    day: '2-digit',
    month: 'short'
  }).format(date)
}
</script>

<style scoped>
.v-card {
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.v-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15) !important;
}
</style>
