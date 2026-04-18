import { Router } from 'express'
import { readFile, writeFile } from 'fs/promises'
import { join } from 'path'
import { Task } from '../types/task'

const router = Router()
const dataPath = join(__dirname, '../data/tasks.json')

async function loadTasks(): Promise<Task[]> {
  const content = await readFile(dataPath, 'utf-8')
  return JSON.parse(content) as Task[]
}

async function saveTasks(tasks: Task[]) {
  await writeFile(dataPath, JSON.stringify(tasks, null, 2), 'utf-8')
}

router.get('/', async (_req, res) => {
  try {
    const tasks = await loadTasks()
    res.json(tasks)
  } catch (error) {
    console.error(error)
    res.status(500).json({ error: 'Impossible de charger les tâches' })
  }
})

router.post('/', async (req, res) => {
  try {
    const { title } = req.body
    if (!title || typeof title !== 'string') {
      return res.status(400).json({ error: 'Le titre est requis' })
    }

    const tasks = await loadTasks()
    const newTask: Task = {
      id: crypto.randomUUID(),
      title: title.trim(),
      completed: false,
      createdAt: new Date().toISOString().slice(0, 10)
    }

    tasks.unshift(newTask)
    await saveTasks(tasks)

    res.status(201).json(newTask)
  } catch (error) {
    console.error(error)
    res.status(500).json({ error: 'Impossible de créer la tâche' })
  }
})

router.patch('/:id', async (req, res) => {
  try {
    const { id } = req.params
    const updates = req.body as Partial<Task>
    const tasks = await loadTasks()
    const task = tasks.find((item) => item.id === id)

    if (!task) {
      return res.status(404).json({ error: 'Tâche introuvable' })
    }

    task.title = typeof updates.title === 'string' ? updates.title : task.title
    task.completed = typeof updates.completed === 'boolean' ? updates.completed : task.completed
    task.createdAt = typeof updates.createdAt === 'string' ? updates.createdAt : task.createdAt

    await saveTasks(tasks)
    res.json(task)
  } catch (error) {
    console.error(error)
    res.status(500).json({ error: 'Impossible de mettre à jour la tâche' })
  }
})

router.delete('/:id', async (req, res) => {
  try {
    const { id } = req.params
    const tasks = await loadTasks()
    const index = tasks.findIndex((item) => item.id === id)

    if (index === -1) {
      return res.status(404).json({ error: 'Tâche introuvable' })
    }

    tasks.splice(index, 1)
    await saveTasks(tasks)
    res.status(204).send()
  } catch (error) {
    console.error(error)
    res.status(500).json({ error: 'Impossible de supprimer la tâche' })
  }
})

export default router
