<template>
  <AdminLayout>
    <v-row class="mb-4">
      <!-- Tâches restantes aujourd'hui -->
      <v-col cols="12" sm="6" md="4">
        <v-card elevation="2" class="pa-6" :to="'/tasks'" style="cursor:pointer;">
          <div class="d-flex align-center">
            <v-avatar color="primary" size="56" class="me-4">
              <v-icon color="white" size="28">mdi-calendar-today</v-icon>
            </v-avatar>
            <div>
              <div class="text-h4 font-weight-bold">{{ todayPending }}</div>
              <div class="text-body-2 text-medium-emphasis">Tâches restantes aujourd'hui</div>
            </div>
          </div>
          <v-progress-linear
            v-if="todayTotal > 0"
            :model-value="todayProgress"
            color="primary"
            rounded
            class="mt-4"
            height="6"
          />
          <div v-if="todayTotal > 0" class="text-caption text-medium-emphasis mt-1">
            {{ todayDone }} / {{ todayTotal }} terminées
          </div>
        </v-card>
      </v-col>

      <!-- Tâches restantes cette semaine -->
      <v-col cols="12" sm="6" md="4">
        <v-card elevation="2" class="pa-6" :to="'/tasks'" style="cursor:pointer;">
          <div class="d-flex align-center">
            <v-avatar color="deep-purple" size="56" class="me-4">
              <v-icon color="white" size="28">mdi-calendar-week</v-icon>
            </v-avatar>
            <div>
              <div class="text-h4 font-weight-bold">{{ weekPending }}</div>
              <div class="text-body-2 text-medium-emphasis">Tâches restantes cette semaine</div>
            </div>
          </div>
          <v-progress-linear
            v-if="weekTotal > 0"
            :model-value="weekProgress"
            color="deep-purple"
            rounded
            class="mt-4"
            height="6"
          />
          <div v-if="weekTotal > 0" class="text-caption text-medium-emphasis mt-1">
            {{ weekDone }} / {{ weekTotal }} terminées
          </div>
        </v-card>
      </v-col>

      <!-- Toutes les tâches en attente -->
      <v-col cols="12" sm="6" md="4">
        <v-card elevation="2" class="pa-6" :to="'/tasks'" style="cursor:pointer;">
          <div class="d-flex align-center">
            <v-avatar color="orange" size="56" class="me-4">
              <v-icon color="white" size="28">mdi-checkbox-marked-circle-outline</v-icon>
            </v-avatar>
            <div>
              <div class="text-h4 font-weight-bold">{{ totalPending }}</div>
              <div class="text-body-2 text-medium-emphasis">Tâches en attente au total</div>
            </div>
          </div>
          <div class="text-caption text-medium-emphasis mt-4">
            {{ totalMinutes }} min de travail planifiées
          </div>
        </v-card>
      </v-col>
    </v-row>
  </AdminLayout>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import AdminLayout from '../layouts/AdminLayout.vue'
import { useTasksStore } from '../stores/tasks'

const tasksStore = useTasksStore()
const { tasks } = storeToRefs(tasksStore)

onMounted(() => {
  if (tasks.value.length === 0) tasksStore.fetchTasks()
})

// --- helpers ---
function startOfDay(d: Date) {
  return new Date(d.getFullYear(), d.getMonth(), d.getDate())
}
function startOfWeek(d: Date) {
  // Lundi 00:00
  const day = d.getDay() === 0 ? 6 : d.getDay() - 1
  const mon = new Date(d)
  mon.setDate(d.getDate() - day)
  return startOfDay(mon)
}
function endOfWeek(d: Date) {
  const sun = new Date(startOfWeek(d))
  sun.setDate(sun.getDate() + 7)
  return sun
}

// --- stats aujourd'hui ---
const todayStart = startOfDay(new Date())
const todayEnd = new Date(todayStart.getTime() + 86400000)

const todayTasks = computed(() =>
  tasks.value.filter(t => {
    if (!t.startAt) return false
    const d = new Date(t.startAt)
    return d >= todayStart && d < todayEnd
  })
)
const todayPending  = computed(() => todayTasks.value.filter(t => !t.finishAt && !t.cancelledAt).length)
const todayDone     = computed(() => todayTasks.value.filter(t => Boolean(t.finishAt)).length)
const todayTotal    = computed(() => todayTasks.value.length)
const todayProgress = computed(() => todayTotal.value ? (todayDone.value / todayTotal.value) * 100 : 0)

// --- stats semaine ---
const weekStart = startOfWeek(new Date())
const weekEnd   = endOfWeek(new Date())

const weekTasks = computed(() =>
  tasks.value.filter(t => {
    if (!t.startAt) return false
    const d = new Date(t.startAt)
    return d >= weekStart && d < weekEnd
  })
)
const weekPending  = computed(() => weekTasks.value.filter(t => !t.finishAt && !t.cancelledAt).length)
const weekDone     = computed(() => weekTasks.value.filter(t => Boolean(t.finishAt)).length)
const weekTotal    = computed(() => weekTasks.value.length)
const weekProgress = computed(() => weekTotal.value ? (weekDone.value / weekTotal.value) * 100 : 0)

// --- toutes tâches en attente ---
const allPending   = computed(() => tasks.value.filter(t => !t.finishAt && !t.cancelledAt))
const totalPending = computed(() => allPending.value.length)
const totalMinutes = computed(() => allPending.value.reduce((s, t) => s + (t.duration ?? 0), 0))
</script>
