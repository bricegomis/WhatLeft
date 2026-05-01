export type RecurrenceType = 'Daily' | 'Weekly'

export interface RecurringTaskTemplate {
  id: string
  title: string
  duration: number
  tags: string[]
  recurrenceType: RecurrenceType
  isActive: boolean
  createdAt: string
}

export interface CreateRecurringTaskTemplateRequest {
  title: string
  duration?: number
  tags?: string[]
  recurrenceType?: RecurrenceType
}

export interface UpdateRecurringTaskTemplateRequest {
  title?: string
  duration?: number
  tags?: string[]
}
