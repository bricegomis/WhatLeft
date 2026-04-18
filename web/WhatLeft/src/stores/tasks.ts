import { defineStore } from 'pinia'
import { TasksApiService } from '../services/tasksApi'

export interface Task {
  id: string
  title: string
  createdAt: string
  duration: number
  finishAt: string | null
}

export const useTasksStore = defineStore('tasks', {
  state: () => ({
    tasks: [] as Task[],
    loading: false,
    error: null as string | null,
    apiAvailable: true
  }),

  getters: {
    finishedTasks: (state) => state.tasks.filter(task => Boolean(task.finishAt)),
    pendingTasks: (state) => state.tasks.filter(task => !task.finishAt),
    isLoading: (state) => state.loading,
    hasError: (state) => state.error !== null,
    isApiAvailable: (state) => state.apiAvailable
  },

  actions: {
    async checkApi() {
      try {
        const ok = await TasksApiService.checkHealth()
        this.apiAvailable = ok
        if (!ok) {
          this.error = 'Le backend n’est pas disponible. Vérifiez que l’API est démarrée.'
        }
        return ok
      } catch (error) {
        this.apiAvailable = false
        this.error = error instanceof Error ? `Backend indisponible : ${error.message}` : 'Backend indisponible'
        console.error('Erreur dans checkApi:', error)
        return false
      }
    },

    async fetchTasks() {
      this.loading = true
      this.error = null
      try {
        const available = await this.checkApi()
        if (!available) {
          return
        }
        this.tasks = await TasksApiService.fetchTasks()
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Erreur lors du chargement des tâches'
        console.error('Erreur dans fetchTasks:', error)
      } finally {
        this.loading = false
      }
    },

    async addTask(title: string, duration = 1, finishAt: string | null = null) {
      if (!title.trim()) return

      this.loading = true
      this.error = null
      try {
        const newTask = await TasksApiService.createTask({
          title: title.trim(),
          duration,
          finishAt
        })
        this.tasks.unshift(newTask)
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Erreur lors de la création de la tâche'
        console.error('Erreur dans addTask:', error)
      } finally {
        this.loading = false
      }
    },

    async toggleFinish(id: string) {
      const task = this.tasks.find((item) => item.id === id)
      if (!task) return

      const newFinishAt = task.finishAt ? null : new Date().toISOString().slice(0, 10)
      const originalFinishAt = task.finishAt

      task.finishAt = newFinishAt

      try {
        await TasksApiService.updateTask(id, { finishAt: newFinishAt })
      } catch (error) {
        task.finishAt = originalFinishAt
        this.error = error instanceof Error ? error.message : 'Erreur lors de la mise à jour de la tâche'
        console.error('Erreur dans toggleFinish:', error)
      }
    },

    async updateTask(id: string, updates: Partial<Task>) {
      const task = this.tasks.find((item) => item.id === id)
      if (!task) return

      const originalTask = { ...task }

      Object.assign(task, updates)

      try {
        const updatedTask = await TasksApiService.updateTask(id, updates)
        Object.assign(task, updatedTask)
      } catch (error) {
        Object.assign(task, originalTask)
        this.error = error instanceof Error ? error.message : 'Erreur lors de la mise à jour de la tâche'
        console.error('Erreur dans updateTask:', error)
      }
    },

    async removeTask(id: string) {
      const taskIndex = this.tasks.findIndex((item) => item.id === id)
      if (taskIndex === -1) return

      const removedTask = this.tasks[taskIndex]

      this.tasks.splice(taskIndex, 1)

      try {
        await TasksApiService.deleteTask(id)
      } catch (error) {
        this.tasks.splice(taskIndex, 0, removedTask)
        this.error = error instanceof Error ? error.message : 'Erreur lors de la suppression de la tâche'
        console.error('Erreur dans removeTask:', error)
      }
    },

    clearError() {
      this.error = null
    }
  }
})
