import { create } from 'zustand'
import { tasksApi } from '../services/apiService'
import type { Task, CreateTaskRequest, UpdateTaskRequest } from '../types/task'

interface TasksState {
  tasks: Task[]
  history: Task[]
  isLoading: boolean
  error: string | null

  // Getters
  pendingTasks: Task[]
  finishedTasks: Task[]
  allTags: string[]

  // Actions
  fetchTasks: () => Promise<void>
  fetchHistory: () => Promise<void>
  addTask: (req: CreateTaskRequest) => Promise<void>
  updateTask: (id: string, req: UpdateTaskRequest) => Promise<void>
  toggleFinish: (id: string) => Promise<void>
  removeTask: (id: string) => Promise<void>
  reactivateTask: (id: string) => Promise<void>
  clearError: () => void
}

export const useTasksStore = create<TasksState>((set, get) => ({
  tasks: [],
  history: [],
  isLoading: false,
  error: null,

  get pendingTasks() {
    return get().tasks.filter(t => !t.finishAt && !t.cancelledAt)
  },
  get finishedTasks() {
    return get().tasks.filter(t => !!t.finishAt)
  },
  get allTags() {
    return [...new Set(get().tasks.flatMap(t => t.tags))].sort()
  },

  fetchTasks: async () => {
    set({ isLoading: true, error: null })
    try {
      const tasks = await tasksApi.getAll()
      set({ tasks })
    } catch (e: any) {
      set({ error: e.message ?? 'Erreur inconnue' })
    } finally {
      set({ isLoading: false })
    }
  },

  fetchHistory: async () => {
    set({ isLoading: true, error: null })
    try {
      const history = await tasksApi.getHistory()
      set({ history })
    } catch (e: any) {
      set({ error: e.message ?? 'Erreur inconnue' })
    } finally {
      set({ isLoading: false })
    }
  },

  addTask: async (req) => {
    set({ isLoading: true, error: null })
    try {
      const task = await tasksApi.create(req)
      set(s => ({ tasks: [task, ...s.tasks] }))
    } catch (e: any) {
      set({ error: e.message })
    } finally {
      set({ isLoading: false })
    }
  },

  updateTask: async (id, req) => {
    try {
      const updated = await tasksApi.update(id, req)
      set(s => ({ tasks: s.tasks.map(t => t.id === id ? updated : t) }))
    } catch (e: any) {
      set({ error: e.message })
    }
  },

  toggleFinish: async (id) => {
    const task = get().tasks.find(t => t.id === id)
    if (!task) return
    // Optimistic update
    const now = new Date().toISOString()
    set(s => ({
      tasks: s.tasks.map(t =>
        t.id === id ? { ...t, finishAt: t.finishAt ? null : now } : t
      ),
    }))
    try {
      await tasksApi.update(id, {})
      await get().fetchTasks() // Sync real state from API
    } catch (e: any) {
      // Rollback
      set(s => ({
        tasks: s.tasks.map(t => t.id === id ? task : t),
        error: e.message,
      }))
    }
  },

  removeTask: async (id) => {
    set(s => ({ tasks: s.tasks.filter(t => t.id !== id) }))
    try {
      await tasksApi.remove(id)
    } catch (e: any) {
      set({ error: e.message })
      await get().fetchTasks()
    }
  },

  reactivateTask: async (id) => {
    try {
      const reactivated = await tasksApi.reactivate(id)
      set(s => ({
        history: s.history.filter(t => t.id !== id),
        tasks: [reactivated, ...s.tasks],
      }))
    } catch (e: any) {
      set({ error: e.message })
    }
  },

  clearError: () => set({ error: null }),
}))
