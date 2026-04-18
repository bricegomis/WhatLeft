<template>
  <AdminLayout>
    <section class="page-header">
      <div>
        <h1>Liste des tâches</h1>
        <p>Suivre et terminer les tâches en cours.</p>
      </div>
      <button class="primary" @click="openModal" :disabled="isLoading">Nouvelle tâche</button>
    </section>

    <!-- Message d'erreur -->
    <div v-if="hasError" class="error-message">
      <p>{{ error }}</p>
      <button @click="retryLoad">Réessayer</button>
      <button @click="clearError">Fermer</button>
    </div>

    <!-- Vérification API -->
    <div v-if="!isApiAvailable" class="error-message">
      <p>Le backend de l'API n'est pas disponible. Démarre le serveur dans le dossier `api/`.</p>
    </div>

    <!-- État de chargement -->
    <div v-else-if="isLoading && tasks.length === 0" class="loading-state">
      <p>Chargement des tâches...</p>
    </div>

    <!-- Liste des tâches -->
    <section v-else-if="tasks.length > 0" class="panel">
      <h2>Tâches ({{ tasks.length }})</h2>
      <table class="table task-list">
        <thead>
          <tr>
            <th>Tâche</th>
            <th>Date</th>
            <th>État</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="task in tasks" :key="task.id" :class="{ completed: task.completed }">
            <td>
              <label class="task-row">
                <input
                  type="checkbox"
                  class="task-checkbox"
                  :checked="task.completed"
                  @change="toggleComplete(task.id)"
                  :disabled="isLoading"
                />
                <span>{{ task.title }}</span>
              </label>
            </td>
            <td>{{ task.createdAt }}</td>
            <td>{{ task.completed ? 'Terminée' : 'En cours' }}</td>
            <td>
              <button
                class="secondary"
                @click="toggleComplete(task.id)"
                :disabled="isLoading"
              >
                {{ task.completed ? 'Reprendre' : 'Terminer' }}
              </button>
              <button
                class="danger"
                @click="removeTask(task.id)"
                :disabled="isLoading"
              >
                Supprimer
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <!-- État vide -->
    <section v-else class="panel empty-state">
      <h2>Aucune tâche</h2>
      <p>Il n'y a pas encore de tâches. Créez votre première tâche !</p>
      <button class="primary" @click="openModal" :disabled="isLoading">Créer une tâche</button>
    </section>

    <!-- Modal pour créer une tâche -->
    <div v-if="isModalOpen" class="modal-overlay" @click.self="closeModal">
      <div class="modal" role="dialog" aria-modal="true" aria-labelledby="modal-title">
        <div class="modal-header">
          <h2 id="modal-title">Ajouter une tâche</h2>
          <button class="close-button" @click="closeModal">×</button>
        </div>

        <div class="form-group">
          <label for="task-title">Titre de la tâche</label>
          <input
            id="task-title"
            v-model="newTaskTitle"
            placeholder="Par exemple : Envoyer le rapport"
            :disabled="isCreating"
          />
        </div>

        <div class="modal-actions">
          <button class="secondary" @click="closeModal" :disabled="isCreating">Annuler</button>
          <button
            class="primary"
            @click="createTask"
            :disabled="!newTaskTitle.trim() || isCreating"
          >
            {{ isCreating ? 'Création...' : 'Créer' }}
          </button>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useTasksStore } from '../stores/tasks'
import AdminLayout from '../layouts/AdminLayout.vue'

const tasksStore = useTasksStore()
const { tasks, isLoading, hasError, error, isApiAvailable } = storeToRefs(tasksStore)

const isModalOpen = ref(false)
const newTaskTitle = ref('')
const isCreating = ref(false)

const openModal = () => {
  newTaskTitle.value = ''
  isModalOpen.value = true
}

const closeModal = () => {
  isModalOpen.value = false
  newTaskTitle.value = ''
}

const createTask = async () => {
  if (!newTaskTitle.value.trim()) return

  isCreating.value = true
  try {
    await tasksStore.addTask(newTaskTitle.value)
    closeModal()
  } catch (error) {
    console.error('Erreur lors de la création:', error)
  } finally {
    isCreating.value = false
  }
}

const toggleComplete = async (id: string) => {
  await tasksStore.toggleComplete(id)
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
