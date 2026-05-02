export interface Task {
  id: string
  title: string
  createdAt: string
  duration: number
  startAt: string | null
  finishAt: string | null
  cancelledAt: string | null
  tags: string[]
  recurringTaskTemplateId: string | null
}

export interface CreateTaskRequest {
  title: string
  duration: number
  startAt?: string | null
  tags?: string[]
}

export interface UpdateTaskRequest {
  title?: string
  duration?: number
  startAt?: string | null
  tags?: string[]
}
