<template>
  <AdminLayout>
    <!-- Page Header -->
    <v-row class="mb-6">
      <v-col cols="12">
        <div class="d-flex flex-column flex-sm-row justify-space-between align-start align-sm-center gap-3">
          <div>
            <h1 class="text-h5 text-sm-h4 font-weight-bold mb-2">Liste des tâches</h1>
            <p class="text-body-2 text-sm-body-1 text-medium-emphasis mb-0">
              Suivre, planifier et terminer les tâches.
            </p>
          </div>
          <v-btn
            color="primary"
            prepend-icon="mdi-plus"
            @click="openModal"
            :disabled="isLoading"
            :block="$vuetify.display.xs"
          >
            Nouvelle tâche
          </v-btn>
        </div>
      </v-col>
    </v-row>

    <!-- Error Messages -->
    <v-alert
      v-if="hasError"
      type="error"
      class="mb-6"
      dismissible
      @click:close="clearError"
    >
      {{ error }}
      <v-btn size="small" variant="text" @click="retryLoad" class="ms-2">
        Réessayer
      </v-btn>
    </v-alert>

    <v-alert
      v-if="!isApiAvailable"
      type="error"
      class="mb-6"
    >
      Le backend de l'API n'est pas disponible. Démarrez le serveur dans le dossier <code>backend/</code>.
    </v-alert>

    <!-- Loading State -->
    <div v-if="isLoading && tasks.length === 0" class="text-center pa-12">
      <v-progress-circular indeterminate color="primary" class="mb-4" />
      <p class="text-body-1">Chargement des tâches...</p>
    </div>

    <!-- Tasks Table -->
    <v-card v-else-if="tasks.length > 0" class="mb-6">
      <v-data-table
        :headers="tableHeaders"
        :items="tasks"
        :loading="isLoading"
        item-key="id"
      >
        <template #item.title="{ item }">
          <div class="d-flex align-center gap-2">
            <v-checkbox
              :checked="!!item.finishAt"
              @change="toggleFinish(item.id)"
              hide-details
              :disabled="isLoading"
            />
            <span :class="{ 'text-decoration-line-through': item.finishAt }">
              {{ item.title }}
            </span>
          </div>
        </template>

        <template #item.createdAt="{ item }">
          {{ formatDate(item.createdAt) }}
        </template>

        <template #item.tags="{ item }">
          <div class="d-flex flex-wrap gap-1 py-1">
            <v-chip
              v-for="tag in item.tags"
              :key="tag"
              size="x-small"
              variant="tonal"
              color="primary"
            >{{ tag }}</v-chip>
            <span v-if="item.tags.length === 0" class="text-medium-emphasis text-caption">—</span>
          </div>
        </template>
        <template #item.actions="{ item }">
          <v-btn
            v-if="!item.finishAt"
            size="small"
            variant="outlined"
            color="success"
            @click="toggleFinish(item.id)"
            :disabled="isLoading"
            class="me-2"
          >
            Terminer
          </v-btn>
          <v-btn
            v-else
            size="small"
            variant="outlined"
            color="primary"
            @click="toggleFinish(item.id)"
            :disabled="isLoading"
            class="me-2"
          >
            Reprendre
          </v-btn>
          <v-btn
            size="small"
            variant="outlined"
            color="error"
            @click="removeTask(item.id)"
            :disabled="isLoading"
            icon="mdi-delete"
          />
        </template>
      </v-data-table>
    </v-card>

    <!-- Empty State -->
    <v-card v-else class="text-center pa-12">
      <v-icon color="primary" size="64" class="mb-4">mdi-checkbox-marked-outline</v-icon>
      <h2 class="text-h6 mb-2">Aucune tâche</h2>
      <p class="text-body-2 text-medium-emphasis mb-6">
        Il n'y a pas encore de tâches. Créez votre première tâche !
      </p>
      <v-btn
        color="primary"
        prepend-icon="mdi-plus"
        @click="openModal"
        :disabled="isLoading"
      >
        Créer une tâche
      </v-btn>
    </v-card>

    <!-- Create Task Dialog -->
    <v-dialog
      v-model="isModalOpen"
      max-width="500"
      :fullscreen="$vuetify.display.xs"
    >
      <v-card>
        <v-card-title>Ajouter une tâche</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="newTaskTitle"
            label="Titre de la tâche"
            placeholder="Par exemple : Envoyer le rapport"
            :disabled="isCreating"
            required
            class="mb-4"
          />

          <v-text-field
            v-model.number="newTaskDuration"
            label="Durée (heures)"
            type="number"
            min="0.5"
            step="0.5"
            :disabled="isCreating"
            class="mb-4"
          />

          <v-text-field
            v-model="newTaskFinishAt"
            label="Terminé le (optionnel)"
            type="date"
            :disabled="isCreating"
            class="mb-4"
          />

          <v-combobox
            v-model="newTaskTags"
            :items="allExistingTags"
            label="Tags"
            placeholder="Sélectionner ou créer des tags…"
            multiple
            chips
            closable-chips
            clearable
            :disabled="isCreating"
            hint="Appuyer sur Entrée pour créer un nouveau tag"
            persistent-hint
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="closeModal" :disabled="isCreating">
            Annuler
          </v-btn>
          <v-btn
            color="primary"
            @click="createTask"
            :disabled="!newTaskTitle.trim() || isCreating"
            :loading="isCreating"
          >
            Créer
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useTasksStore } from '../stores/tasks'
import AdminLayout from '../layouts/AdminLayout.vue'

const tasksStore = useTasksStore()
const { tasks, isLoading, hasError, error, isApiAvailable } = storeToRefs(tasksStore)

const isModalOpen = ref(false)
const newTaskTitle = ref('')
const newTaskDuration = ref(1)
const newTaskFinishAt = ref('')
const newTaskTags = ref<string[]>([])
const isCreating = ref(false)

// Tous les tags déjà utilisés dans les tâches existantes (dédupliqués, triés)
const allExistingTags = computed(() => {
  const set = new Set<string>()
  for (const task of tasks.value) {
    for (const tag of task.tags) set.add(tag)
  }
  return [...set].sort()
})

const tableHeaders = [
  { title: 'Tâche', key: 'title', width: '40%' },
  { title: 'Tags', key: 'tags', width: '25%', sortable: false },
  { title: 'Créé le', key: 'createdAt', width: '15%' },
  { title: 'Actions', key: 'actions', width: '10%', sortable: false }
]

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('fr-FR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const openModal = () => {
  newTaskTitle.value = ''
  newTaskDuration.value = 1
  newTaskFinishAt.value = ''
  newTaskTags.value = []
  isModalOpen.value = true
}

const closeModal = () => {
  isModalOpen.value = false
  newTaskTitle.value = ''
  newTaskDuration.value = 1
  newTaskFinishAt.value = ''
  newTaskTags.value = []
}

const createTask = async () => {
  if (!newTaskTitle.value.trim()) return

  isCreating.value = true
  try {
    await tasksStore.addTask(
      newTaskTitle.value,
      newTaskDuration.value,
      null,
      newTaskFinishAt.value || null,
      newTaskTags.value
    )
    closeModal()
  } catch (error) {
    console.error('Erreur lors de la création:', error)
  } finally {
    isCreating.value = false
  }
}

const toggleFinish = async (id: string) => {
  await tasksStore.toggleFinish(id)
}

const removeTask = async (id: string) => {
  if (confirm('Êtes-vous sûr de vouloir supprimer cette tâche ?')) {
    await tasksStore.removeTask(id)
  }
}

const retryLoad = () => {
  tasksStore.fetchTasks()
}

const clearError = () => {
  tasksStore.clearError()
}

onMounted(() => {
  tasksStore.fetchTasks()
})
</script>
