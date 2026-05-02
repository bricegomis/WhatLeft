import { defineStore } from 'pinia'
import { RecurringApiService } from '../services/recurringApi'
import type { RecurringTaskTemplate, CreateRecurringTaskTemplateRequest, UpdateRecurringTaskTemplateRequest } from '../types/recurring'

export const useRecurringStore = defineStore('recurring', {
  state: () => ({
    templates: [] as RecurringTaskTemplate[],
    loading: false,
    error: null as string | null
  }),

  getters: {
    activeTemplates: (state) => state.templates.filter(t => t.isActive),
    inactiveTemplates: (state) => state.templates.filter(t => !t.isActive),
    isLoading: (state) => state.loading,
    hasError: (state) => state.error !== null
  },

  actions: {
    async fetchTemplates() {
      this.loading = true
      this.error = null
      try {
        this.templates = await RecurringApiService.fetchTemplates()
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Erreur lors du chargement des récurrences'
      } finally {
        this.loading = false
      }
    },

    async addTemplate(data: CreateRecurringTaskTemplateRequest) {
      this.loading = true
      this.error = null
      try {
        const created = await RecurringApiService.createTemplate(data)
        this.templates.unshift(created)
        return created
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Erreur lors de la création'
      } finally {
        this.loading = false
      }
    },

    async updateTemplate(id: string, data: UpdateRecurringTaskTemplateRequest) {
      this.error = null
      try {
        const updated = await RecurringApiService.updateTemplate(id, data)
        const idx = this.templates.findIndex(t => t.id === id)
        if (idx !== -1) this.templates[idx] = updated
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Erreur lors de la mise à jour'
      }
    },

    async deactivateTemplate(id: string) {
      this.error = null
      try {
        await RecurringApiService.deleteTemplate(id)
        const t = this.templates.find(t => t.id === id)
        if (t) t.isActive = false
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Erreur lors de la désactivation'
      }
    },

    async activateTemplate(id: string) {
      this.error = null
      try {
        await RecurringApiService.activateTemplate(id)
        const t = this.templates.find(t => t.id === id)
        if (t) t.isActive = true
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Erreur lors de l\'activation'
      }
    },

    async processNow(id: string) {
      this.error = null
      try {
        await RecurringApiService.processNow(id)
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'Erreur lors du déclenchement'
      }
    },

    async advance(id: string) {
      this.error = null
      try {
        await RecurringApiService.advance(id)
      } catch (e) {
        this.error = e instanceof Error ? e.message : "Erreur lors de l'avancement"
      }
    },

    async advanceAllByType(type: 'Daily' | 'Weekly' | 'Monthly') {
      this.error = null
      try {
        await RecurringApiService.advanceAllByType(type)
      } catch (e) {
        this.error = e instanceof Error ? e.message : "Erreur lors de l'avancement global"
      }
    },

    clearError() {
      this.error = null
    }
  }
})
