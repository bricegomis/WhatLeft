<template>
  <AdminLayout>
    <section class="page-header">
      <div>
        <h1>Liste des tâches</h1>
        <p>Suivre, planifier et terminer les tâches avec un modèle plus riche.</p>
      </div>
      <button class="primary" @click="openModal" :disabled="isLoading">Nouvelle tâche</button>
    </section>

    <div v-if="hasError" class="error-message">
      <p>{{ error }}</p>
      <button @click="retryLoad">Réessayer</button>
      <button @click="clearError">Fermer</button>
    </div>

    <div v-if="!isApiAvailable" class="error-message">
      <p>Le backend de l'API n'est pas disponible. Démarre le serveur dans le dossier `api/`.</p>
    </div>

    <div v-else-if="isLoading && tasks.length === 0" class="loading-state">
      <p>Chargement des tâches...</p>
    </div>

    <section v-else-if="tasks.length > 0" class="panel">
      <h2>Tâches ({{ tasks.length }})</h2>
      <table class="table task-list">
        <thead>
          <tr>
            <th>Tâche</th>
            <th>Créé le</th>
            <th>Durée</th>
            <th>Terminé le</th>
            <th>Statut</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="task in tasks" :key="task.id" :class="{ completed: !!task.finishAt }">
            <td>
              <label class="task-row">
                <input
                  type="checkbox"
                  class="task-checkbox"
                  :checked="!!task.finishAt"
                  @change="toggleFinish(task.id)"
                  :disabled="isLoading"
                />
                <span>{{ task.title }}</span>
              </label>
            </td>
            <td>{{ task.createdAt }}</td>
            <td>{{ task.duration }} h</td>
            <td>{{ task.finishAt || '—' }}</td>
            <td>{{ task.finishAt ? 'Terminée' : 'En cours' }}</td>
            <td>
              <button class="secondary" @click="toggleFinish(task.id)" :disabled="isLoading">
                {{ task.finishAt ? 'Reprendre' : 'Terminer' }}
              </button>
              <button class="danger" @click="removeTask(task.id)" :disabled="isLoading">
                Supprimer
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <section v-else class="panel empty-state">
      <h2>Aucune tâche</h2>
      <p>Il n'y a pas encore de tâches. Créez votre première tâche !</p>
      <button class="primary" @click="openModal" :disabled="isLoading">Créer une tâche</button>
    </section>

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

        <div class="form-group">
          <label for="task-duration">Durée (heures)</label>
          <input
            id="task-duration"
            type="number"
            min="0.5"
            step="0.5"
            v-model.number="newTaskDuration"
            :disabled="isCreating"
          />
        </div>

        <div class="form-group">
          <label for="task-finish-at">Terminé le (optionnel)</label>
          <input
            id="task-finish-at"
            type="date"
            v-model="newTaskFinishAt"
            :disabled="isCreating"
          />
        </div>

        <div class="modal-actions">
          <button class="secondary" @click="closeModal" :disabled="isCreating">Annuler</button>
          <button class="primary" @click="createTask" :disabled="!newTaskTitle.trim() || isCreating">
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
const newTaskDuration = ref(1)
const newTaskFinishAt = ref('')
const isCreating = ref(false)

const openModal = () => {
  newTaskTitle.value = ''
  newTaskDuration.value = 1
  newTaskFinishAt.value = ''
  isModalOpen.value = true
}

const closeModal = () => {
  isModalOpen.value = false
  newTaskTitle.value = ''
  newTaskDuration.value = 1
  newTaskFinishAt.value = ''
}

const createTask = async () => {
  if (!newTaskTitle.value.trim()) return

  isCreating.value = true
  try {
    await tasksStore.addTask(
      newTaskTitle.value,
      newTaskDuration.value,
      newTaskFinishAt.value || null
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
