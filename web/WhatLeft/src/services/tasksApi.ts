import type { Task } from '../stores/tasks'

const API_BASE_URL = 'https://jsonplaceholder.typicode.com/todos'

export class TasksApiService {
  static async fetchTasks(): Promise<Task[]> {
    try {
      const response = await fetch(`${API_BASE_URL}?_limit=10`)
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
      const todos = await response.json()

      // Convertir les todos de JSONPlaceholder vers notre format Task
      return todos.map((todo: any) => ({
        id: todo.id.toString(),
        title: todo.title,
        completed: todo.completed,
        createdAt: new Date().toISOString().slice(0, 10) // Date d'aujourd'hui par défaut
      }))
    } catch (error) {
      console.error('Erreur lors de la récupération des tâches:', error)
      throw error
    }
  }

  static async createTask(title: string): Promise<Task> {
    try {
      const response = await fetch(API_BASE_URL, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          title,
          completed: false,
          userId: 1
        })
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const todo = await response.json()
      return {
        id: todo.id.toString(),
        title: todo.title,
        completed: todo.completed,
        createdAt: new Date().toISOString().slice(0, 10)
      }
    } catch (error) {
      console.error('Erreur lors de la création de la tâche:', error)
      throw error
    }
  }

  static async updateTask(id: string, updates: Partial<Task>): Promise<Task> {
    try {
      const response = await fetch(`${API_BASE_URL}/${id}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          title: updates.title,
          completed: updates.completed
        })
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const todo = await response.json()
      return {
        id: todo.id.toString(),
        title: todo.title,
        completed: todo.completed,
        createdAt: updates.createdAt || new Date().toISOString().slice(0, 10)
      }
    } catch (error) {
      console.error('Erreur lors de la mise à jour de la tâche:', error)
      throw error
    }
  }

  static async deleteTask(id: string): Promise<void> {
    try {
      const response = await fetch(`${API_BASE_URL}/${id}`, {
        method: 'DELETE'
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
    } catch (error) {
      console.error('Erreur lors de la suppression de la tâche:', error)
      throw error
    }
  }
}