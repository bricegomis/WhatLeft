import { defineStore } from 'pinia'
import { RecurringApiService } from '../services/recurringApi'
import { TasksApiService } from '../services/tasksApi'
import { useTasksStore } from './tasks'
import type { Task } from './tasks'

export const useHistoryStore = defineStore('history', {
  state: () => ({
    items: [] as Task[],
    loading: false,
    error: null as string | null
  }),

  getters: {
    finishedItems: (state) => state.items.filter(t => t.finishAt && !t.cancelledAt),
    cancelledItems: (state) => state.items.filter(t => t.cancelledAt),
    isLoading: (state) => state.loading,
    hasError: (state) => state.error !== null
  },

  actions: {
    async fetchHistory() {
      this.loading = true
      this.error = null
      try {
        this.items = await RecurringApiService.fetchHistory()
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Erreur lors du chargement de l\'historique'
      } finally {
        this.loading = false
      }
    },

    clearError() {
      this.error = null
    },

    async reactivateTask(id: string) {
      this.error = null
      try {
        await TasksApiService.reactivateTask(id)
        this.items = this.items.filter(t => t.id !== id)
        // Refresh active tasks list
        useTasksStore().fetchTasks()
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Erreur lors de la réactivation'
      }
    }
  }
})
