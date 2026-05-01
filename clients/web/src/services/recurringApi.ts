import type { RecurringTaskTemplate, CreateRecurringTaskTemplateRequest, UpdateRecurringTaskTemplateRequest } from '../types/recurring'
import type { Task } from '../stores/tasks'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'

let authToken: string | null = null

function authHeaders(): HeadersInit {
  return authToken
    ? { 'Content-Type': 'application/json', Authorization: `Bearer ${authToken}` }
    : { 'Content-Type': 'application/json' }
}

export function setRecurringAuthToken(token: string | null) {
  authToken = token
}

export class RecurringApiService {
  static async fetchTemplates(): Promise<RecurringTaskTemplate[]> {
    const response = await fetch(`${API_BASE_URL}/recurring-templates`, {
      headers: authHeaders()
    })
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`)
    return response.json()
  }

  static async createTemplate(data: CreateRecurringTaskTemplateRequest): Promise<RecurringTaskTemplate> {
    const response = await fetch(`${API_BASE_URL}/recurring-templates`, {
      method: 'POST',
      headers: authHeaders(),
      body: JSON.stringify(data)
    })
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`)
    return response.json()
  }

  static async updateTemplate(id: string, data: UpdateRecurringTaskTemplateRequest): Promise<RecurringTaskTemplate> {
    const response = await fetch(`${API_BASE_URL}/recurring-templates/${id}`, {
      method: 'PUT',
      headers: authHeaders(),
      body: JSON.stringify(data)
    })
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`)
    return response.json()
  }

  static async deleteTemplate(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/recurring-templates/${id}`, {
      method: 'DELETE',
      headers: authToken ? { Authorization: `Bearer ${authToken}` } : {}
    })
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`)
  }

  static async activateTemplate(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/recurring-templates/${id}/activate`, {
      method: 'POST',
      headers: authHeaders()
    })
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`)
  }

  static async processNow(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/recurring-templates/${id}/process`, {
      method: 'POST',
      headers: authHeaders()
    })
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`)
  }

  static async fetchHistory(): Promise<Task[]> {
    const response = await fetch(`${API_BASE_URL}/tasks/history`, {
      headers: authHeaders()
    })
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`)
    return response.json()
  }
}
