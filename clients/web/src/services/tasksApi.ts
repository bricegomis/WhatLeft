import type { Task } from '../stores/tasks'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'

let authToken: string | null = null

export function setAuthToken(token: string | null) {
  authToken = token
}

function authHeaders(): HeadersInit {
  return authToken
    ? { 'Content-Type': 'application/json', Authorization: `Bearer ${authToken}` }
    : { 'Content-Type': 'application/json' }
}

export class TasksApiService {
  static async checkHealth(): Promise<boolean> {
    try {
      // A 401 means the API is up but needs authentication — not a health failure
      const response = await fetch(`${API_BASE_URL}/tasks`, { headers: authHeaders() })
      return response.ok || response.status === 401
    } catch {
      return false
    }
  }

  static async fetchTasks(): Promise<Task[]> {
    const response = await fetch(`${API_BASE_URL}/tasks`, {
      headers: authHeaders()
    })
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
    return response.json()
  }

  static async createTask(data: { title: string; duration: number; startAt: string | null; finishAt: string | null; tags?: string[] }): Promise<Task> {
    const response = await fetch(`${API_BASE_URL}/tasks`, {
      method: 'POST',
      headers: authHeaders(),
      body: JSON.stringify(data)
    })
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
    return response.json()
  }

  static async updateTask(id: string, updates: Partial<Task>): Promise<Task> {
    const response = await fetch(`${API_BASE_URL}/tasks/${id}`, {
      method: 'PUT',
      headers: authHeaders(),
      body: JSON.stringify(updates)
    })
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
    return response.json()
  }

  static async deleteTask(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/tasks/${id}`, {
      method: 'DELETE',
      headers: authToken ? { Authorization: `Bearer ${authToken}` } : {}
    })
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
  }

  static async reactivateTask(id: string): Promise<Task> {
    const response = await fetch(`${API_BASE_URL}/tasks/${id}/reactivate`, {
      method: 'POST',
      headers: authHeaders()
    })
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
    return response.json()
  }
}
