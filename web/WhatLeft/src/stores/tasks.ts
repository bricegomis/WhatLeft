import { defineStore } from 'pinia'

export interface Task {
  id: string
  title: string
  completed: boolean
  createdAt: string
}

export const useTasksStore = defineStore('tasks', {
  state: () => ({
    tasks: [
      { id: '1', title: 'Préparer le rapport hebdo', completed: false, createdAt: '2026-04-18' },
      { id: '2', title: 'Relire les demandes clients', completed: true, createdAt: '2026-04-17' },
      { id: '3', title: 'Planifier la réunion produit', completed: false, createdAt: '2026-04-20' },
      { id: '4', title: 'Mettre à jour la documentation', completed: false, createdAt: '2026-04-22' },
      { id: '5', title: 'Réunion équipe', completed: false, createdAt: '2026-04-25' }
    ] as Task[]
  }),
  actions: {
    addTask(title: string) {
      if (!title.trim()) return
      this.tasks.unshift({
        id: Date.now().toString(),
        title: title.trim(),
        completed: false,
        createdAt: new Date().toISOString().slice(0, 10)
      })
    },
    toggleComplete(id: string) {
      const task = this.tasks.find((item) => item.id === id)
      if (task) task.completed = !task.completed
    },
    removeTask(id: string) {
      this.tasks = this.tasks.filter((item) => item.id !== id)
    }
  }
})
