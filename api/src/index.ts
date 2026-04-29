import 'dotenv/config'
import express from 'express'
import cors from 'cors'
import { auth } from 'express-oauth2-jwt-bearer'
import tasksRouter from './routes/tasks'

const app = express()
const PORT = process.env.PORT ? Number(process.env.PORT) : 3000

const checkJwt = auth({
  audience: process.env.AUTH0_AUDIENCE,
  issuerBaseURL: `https://${process.env.AUTH0_DOMAIN}/`,
})

app.use(cors())
app.use(express.json())

app.get('/', (_req, res) => {
  res.json({ status: 'ok', message: 'WhatLeft API backend is running' })
})

app.use('/tasks', checkJwt, tasksRouter)

app.listen(PORT, () => {
  console.log(`WhatLeft API listening at http://localhost:${PORT}`)
})
