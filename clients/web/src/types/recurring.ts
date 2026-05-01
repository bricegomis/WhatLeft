export type RecurrenceType = 'Daily' | 'Weekly'

export interface RecurringTemplate {
  id: string
  title: string
  duration: number
  tags: string[]
  recurrenceType: RecurrenceType
  isActive: boolean
  createdAt: string
}

export interface CreateRecurringTemplateRequest {
  title: string
  duration?: number
  tags?: string[]
  recurrenceType?: RecurrenceType
}

export interface UpdateRecurringTemplateRequest {
  title?: string
  duration?: number
  tags?: string[]
}
