import { create } from 'zustand'
import { recurringApi } from '../services/apiService'
import type { RecurringTemplate, CreateRecurringTemplateRequest, UpdateRecurringTemplateRequest, RecurrenceType } from '../types/recurring'

interface RecurringState {
  templates: RecurringTemplate[]
  isLoading: boolean
  error: string | null

  activeTemplates: RecurringTemplate[]
  inactiveTemplates: RecurringTemplate[]

  fetchTemplates: () => Promise<void>
  addTemplate: (req: CreateRecurringTemplateRequest) => Promise<void>
  updateTemplate: (id: string, req: UpdateRecurringTemplateRequest) => Promise<void>
  deactivateTemplate: (id: string) => Promise<void>
  activateTemplate: (id: string) => Promise<void>
  advance: (id: string) => Promise<void>
  advanceAllByType: (type: RecurrenceType) => Promise<void>
  processNow: (id: string) => Promise<void>
  clearError: () => void
}

export const useRecurringStore = create<RecurringState>((set, get) => ({
  templates: [],
  isLoading: false,
  error: null,

  get activeTemplates() {
    return get().templates.filter(t => t.isActive)
  },
  get inactiveTemplates() {
    return get().templates.filter(t => !t.isActive)
  },

  fetchTemplates: async () => {
    set({ isLoading: true, error: null })
    try {
      set({ templates: await recurringApi.getAll() })
    } catch (e: any) {
      set({ error: e.message })
    } finally {
      set({ isLoading: false })
    }
  },

  addTemplate: async (req) => {
    set({ isLoading: true, error: null })
    try {
      const tpl = await recurringApi.create(req)
      set(s => ({ templates: [tpl, ...s.templates] }))
    } catch (e: any) {
      set({ error: e.message })
    } finally {
      set({ isLoading: false })
    }
  },

  updateTemplate: async (id, req) => {
    try {
      const updated = await recurringApi.update(id, req)
      set(s => ({ templates: s.templates.map(t => t.id === id ? updated : t) }))
    } catch (e: any) {
      set({ error: e.message })
    }
  },

  deactivateTemplate: async (id) => {
    try {
      await recurringApi.remove(id)
      set(s => ({
        templates: s.templates.map(t => t.id === id ? { ...t, isActive: false } : t),
      }))
    } catch (e: any) {
      set({ error: e.message })
    }
  },

  activateTemplate: async (id) => {
    try {
      await recurringApi.activate(id)
      set(s => ({
        templates: s.templates.map(t => t.id === id ? { ...t, isActive: true } : t),
      }))
    } catch (e: any) {
      set({ error: e.message })
    }
  },

  advance: async (id) => {
    set({ isLoading: true, error: null })
    try {
      await recurringApi.advance(id)
    } catch (e: any) {
      set({ error: e.message })
    } finally {
      set({ isLoading: false })
    }
  },

  advanceAllByType: async (type) => {
    set({ isLoading: true, error: null })
    try {
      await recurringApi.advanceAllByType(type)
    } catch (e: any) {
      set({ error: e.message })
    } finally {
      set({ isLoading: false })
    }
  },

  processNow: async (id) => {
    set({ isLoading: true, error: null })
    try {
      await recurringApi.processNow(id)
    } catch (e: any) {
      set({ error: e.message })
    } finally {
      set({ isLoading: false })
    }
  },

  clearError: () => set({ error: null }),
}))
