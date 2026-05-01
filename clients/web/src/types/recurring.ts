export type RecurrenceType = 'Daily' | 'Weekly'

export interface RecurringTemplate {
  id: string
  title: string
  duration: number
  tags: string[]
  recurrenceType: RecurrenceType
  frequencyPerPeriod: number
  resetHour: number
  isActive: boolean
  createdAt: string
}

export interface CreateRecurringTemplateRequest {
  title: string
  duration?: number
  tags?: string[]
  recurrenceType?: RecurrenceType
  frequencyPerPeriod?: number
  resetHour?: number
}

export interface UpdateRecurringTemplateRequest {
  title?: string
  duration?: number
  tags?: string[]
  frequencyPerPeriod?: number
  resetHour?: number
}
