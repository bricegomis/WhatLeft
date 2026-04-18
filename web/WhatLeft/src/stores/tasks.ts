import { defineStore } from 'pinia'
import { TasksApiService } from '../services/tasksApi'

export interface Task {
  id: string
  title: string
  completed: boolean
  createdAt: string
}

export const useTasksStore = defineStore('tasks', {
  state: () => ({
    tasks: [] as Task[],
    loading: false,
    error: null as string | null
  }),

  getters: {
    completedTasks: (state) => state.tasks.filter(task => task.completed),
    pendingTasks: (state) => state.tasks.filter(task => !task.completed),
    isLoading: (state) => state.loading,
    hasError: (state) => state.error !== null
  },

  actions: {
    async fetchTasks() {
      this.loading = true
      this.error = null
      try {
        this.tasks = await TasksApiService.fetchTasks()
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Erreur lors du chargement des tâches'
        console.error('Erreur dans fetchTasks:', error)
      } finally {
        this.loading = false
      }
    },

    async addTask(title: string) {
      if (!title.trim()) return

      this.loading = true
      this.error = null
      try {
        const newTask = await TasksApiService.createTask(title.trim())
        this.tasks.unshift(newTask)
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Erreur lors de la création de la tâche'
        console.error('Erreur dans addTask:', error)
      } finally {
        this.loading = false
      }
    },

    async toggleComplete(id: string) {
      const task = this.tasks.find((item) => item.id === id)
      if (!task) return

      const newCompleted = !task.completed
      const originalCompleted = task.completed

      // Optimistic update
      task.completed = newCompleted

      try {
        await TasksApiService.updateTask(id, { completed: newCompleted })
      } catch (error) {
        // Revert on error
        task.completed = originalCompleted
        this.error = error instanceof Error ? error.message : 'Erreur lors de la mise à jour de la tâche'
        console.error('Erreur dans toggleComplete:', error)
      }
    },

    async updateTask(id: string, updates: Partial<Task>) {
      const task = this.tasks.find((item) => item.id === id)
      if (!task) return

      const originalTask = { ...task }

      // Optimistic update
      Object.assign(task, updates)

      try {
        const updatedTask = await TasksApiService.updateTask(id, updates)
        // Replace with server response
        Object.assign(task, updatedTask)
      } catch (error) {
        // Revert on error
        Object.assign(task, originalTask)
        this.error = error instanceof Error ? error.message : 'Erreur lors de la mise à jour de la tâche'
        console.error('Erreur dans updateTask:', error)
      }
    },

    async removeTask(id: string) {
      const taskIndex = this.tasks.findIndex((item) => item.id === id)
      if (taskIndex === -1) return

      const removedTask = this.tasks[taskIndex]

      // Optimistic update
      this.tasks.splice(taskIndex, 1)

      try {
        await TasksApiService.deleteTask(id)
      } catch (error) {
        // Revert on error
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
