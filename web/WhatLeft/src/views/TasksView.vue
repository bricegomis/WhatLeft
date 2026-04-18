<template>
  <AdminLayout>
    <section class="page-header">
      <div>
        <h1>Liste des tâches</h1>
        <p>Suivre et terminer les tâches en cours.</p>
      </div>
      <button class="primary" @click="openModal">Nouvelle tâche</button>
    </section>

    <section class="panel">
      <h2>Tâches</h2>
      <div v-if="tasks.length === 0" class="empty-state">
        <p>Aucune tâche pour le moment. Ajoute une nouvelle tâche pour commencer.</p>
      </div>
      <table v-else class="table task-list">
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
                <input type="checkbox" class="task-checkbox" :checked="task.completed" @change="toggleComplete(task.id)" />
                <span>{{ task.title }}</span>
              </label>
            </td>
            <td>{{ task.createdAt }}</td>
            <td>{{ task.completed ? 'Terminée' : 'En cours' }}</td>
            <td>
              <button class="secondary" @click="toggleComplete(task.id)">
                {{ task.completed ? 'Reprendre' : 'Terminer' }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <div v-if="isModalOpen" class="modal-overlay" @click.self="closeModal">
      <div class="modal" role="dialog" aria-modal="true" aria-labelledby="modal-title">
        <div class="modal-header">
          <h2 id="modal-title">Ajouter une tâche</h2>
          <button class="close-button" @click="closeModal">×</button>
        </div>

        <div class="form-group">
          <label for="task-title">Titre de la tâche</label>
          <input id="task-title" v-model="newTaskTitle" placeholder="Par exemple : Envoyer le rapport" />
        </div>

        <div class="modal-actions">
          <button class="secondary" @click="closeModal">Annuler</button>
          <button class="primary" @click="createTask">Créer</button>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useTasksStore } from '../stores/tasks'
import AdminLayout from '../layouts/AdminLayout.vue'

const tasksStore = useTasksStore()
const tasks = tasksStore.tasks
const isModalOpen = ref(false)
const newTaskTitle = ref('')

const openModal = () => {
  newTaskTitle.value = ''
  isModalOpen.value = true
}

const closeModal = () => {
  isModalOpen.value = false
}

const createTask = () => {
  tasksStore.addTask(newTaskTitle.value)
  closeModal()
}

const toggleComplete = (id: string) => {
  tasksStore.toggleComplete(id)
}
</script>
