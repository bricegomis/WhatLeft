<template>
  <AdminLayout>
    <v-alert v-if="hasError" type="error" class="mb-6" dismissible @click:close="clearError">
      {{ error }}
    </v-alert>

    <div v-if="isLoading && items.length === 0" class="text-center pa-12">
      <v-progress-circular indeterminate color="primary" class="mb-4" />
      <p class="text-body-1">Chargement de l'historique...</p>
    </div>

    <!-- Onglets Terminées / Annulées -->
    <v-tabs v-model="tab" class="mb-4" color="primary">
      <v-tab value="finished">
        <v-icon start>mdi-check-circle-outline</v-icon>
        Terminées
        <v-chip size="x-small" class="ms-2" color="success" variant="tonal">{{ finishedItems.length }}</v-chip>
      </v-tab>
      <v-tab value="cancelled">
        <v-icon start>mdi-close-circle-outline</v-icon>
        Non faites
        <v-chip size="x-small" class="ms-2" color="error" variant="tonal">{{ cancelledItems.length }}</v-chip>
      </v-tab>
    </v-tabs>

    <v-window v-model="tab">
      <!-- Terminées -->
      <v-window-item value="finished">
        <v-card v-if="finishedItems.length > 0">
          <v-list lines="two">
            <template v-for="(task, idx) in finishedItems" :key="task.id">
              <v-list-item>
                <template #prepend>
                  <v-icon color="success">mdi-check-circle</v-icon>
                </template>
                <v-list-item-title class="text-decoration-line-through text-medium-emphasis">
                  {{ task.title }}
                </v-list-item-title>
                <v-list-item-subtitle>
                  Terminé le {{ formatDate(task.finishAt!) }}
                  <span v-if="task.recurringTaskTemplateId" class="ms-1">
                    · <v-icon size="12" color="primary">mdi-repeat</v-icon> récurrente
                  </span>
                  <span v-if="task.tags.length" class="ms-1">
                    ·
                    <v-chip
                      v-for="tag in task.tags"
                      :key="tag"
                      size="x-small"
                      variant="tonal"
                      color="primary"
                      class="me-1"
                    >{{ tag }}</v-chip>
                  </span>
                </v-list-item-subtitle>
              </v-list-item>
              <v-divider v-if="idx < finishedItems.length - 1" inset />
            </template>
          </v-list>
        </v-card>
        <v-card v-else class="text-center pa-12">
          <v-icon size="48" color="grey" class="mb-3">mdi-check-circle-outline</v-icon>
          <p class="text-body-2 text-medium-emphasis">Aucune tâche terminée.</p>
        </v-card>
      </v-window-item>

      <!-- Annulées / Non faites -->
      <v-window-item value="cancelled">
        <v-card v-if="cancelledItems.length > 0">
          <v-list lines="two">
            <template v-for="(task, idx) in cancelledItems" :key="task.id">
              <v-list-item>
                <template #prepend>
                  <v-icon color="error">mdi-close-circle</v-icon>
                </template>
                <v-list-item-title class="text-medium-emphasis">{{ task.title }}</v-list-item-title>
                <v-list-item-subtitle>
                  Non faite — annulée le {{ formatDate(task.cancelledAt!) }}
                  <span v-if="task.tags.length" class="ms-1">
                    ·
                    <v-chip
                      v-for="tag in task.tags"
                      :key="tag"
                      size="x-small"
                      variant="tonal"
                      color="error"
                      class="me-1"
                    >{{ tag }}</v-chip>
                  </span>
                </v-list-item-subtitle>
              </v-list-item>
              <v-divider v-if="idx < cancelledItems.length - 1" inset />
            </template>
          </v-list>
        </v-card>
        <v-card v-else class="text-center pa-12">
          <v-icon size="48" color="grey" class="mb-3">mdi-close-circle-outline</v-icon>
          <p class="text-body-2 text-medium-emphasis">Aucune tâche non faite.</p>
        </v-card>
      </v-window-item>
    </v-window>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import AdminLayout from '../layouts/AdminLayout.vue'
import { useHistoryStore } from '../stores/history'

const historyStore = useHistoryStore()
const { items, isLoading, hasError, error, finishedItems, cancelledItems } = storeToRefs(historyStore)

const tab = ref('finished')

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleDateString('fr-FR', {
    day: 'numeric',
    month: 'long',
    year: 'numeric'
  })
}

function clearError() {
  historyStore.clearError()
}

onMounted(() => {
  historyStore.fetchHistory()
})
</script>
