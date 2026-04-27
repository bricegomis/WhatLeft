export interface Task {
  id: string
  title: string
  createdAt: string
  duration: number
  startAt: string | null
  finishAt: string | null
  tags: string[]
}
