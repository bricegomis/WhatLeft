import express from 'express'
import cors from 'cors'
import tasksRouter from './routes/tasks'

const app = express()
const PORT = process.env.PORT ? Number(process.env.PORT) : 3000

app.use(cors())
app.use(express.json())

app.use('/api/tasks', tasksRouter)

app.get('/', (_req, res) => {
  res.json({ status: 'ok', message: 'WhatLeft API backend is running' })
})

app.listen(PORT, () => {
  console.log(`WhatLeft API listening at http://localhost:${PORT}`)
})
