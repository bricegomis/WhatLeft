<template>
  <AdminLayout>
    <section class="page-header">
      <div>
        <h1>Calendrier</h1>
        <p>Planifier et organiser vos tâches par date.</p>
      </div>
    </section>

    <section class="panel calendar-panel">
      <VueCalendar
        ref="calendarRef"
        :options="calendarOptions"
        :events="calendarEvents"
        @eventDrop="handleEventDrop"
        @eventClick="handleEventClick"
      />
    </section>

    <!-- Modal pour modifier la tâche -->
    <div v-if="selectedEvent" class="modal-overlay" @click.self="closeEventModal">
      <div class="modal">
        <div class="modal-header">
          <h2>Modifier la tâche</h2>
          <button class="close-button" @click="closeEventModal">×</button>
        </div>

        <div class="form-group">
          <label for="event-title">Titre</label>
          <input id="event-title" v-model="selectedEvent!.title" />
        </div>

        <div class="form-group">
          <label for="event-date">Date</label>
          <input id="event-date" type="date" v-model="selectedEvent!.start" />
        </div>

        <div class="form-group">
          <label>
            <input type="checkbox" v-model="selectedEvent!.completed" />
            Terminée
          </label>
        </div>

        <div class="modal-actions">
          <button class="secondary" @click="closeEventModal">Annuler</button>
          <button class="primary" @click="updateEvent">Sauvegarder</button>
        </div>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'
import VueCalendar from '@fullcalendar/vue3'
import { useTasksStore } from '../stores/tasks'
import AdminLayout from '../layouts/AdminLayout.vue'

interface SelectedEvent {
  id: string
  title: string
  start: string
  completed: boolean
}

const tasksStore = useTasksStore()
const calendarRef = ref()
const selectedEvent = ref<SelectedEvent | null>(null)

const calendarOptions = {
  plugins: [dayGridPlugin, interactionPlugin],
  initialView: 'dayGridMonth',
  editable: true,
  droppable: true,
  headerToolbar: {
    left: 'prev,next today',
    center: 'title',
    right: 'dayGridMonth,dayGridWeek'
  },
  locale: 'fr',
  firstDay: 1, // Lundi
  height: 'auto',
  eventDisplay: 'block',
  eventColor: '#4f46e5',
  eventTextColor: '#ffffff'
}

const calendarEvents = computed(() => {
  return tasksStore.tasks.map(task => ({
    id: task.id,
    title: task.title,
    start: task.createdAt,
    extendedProps: {
      completed: task.completed
    },
    backgroundColor: task.completed ? '#16a34a' : '#4f46e5'
  }))
})

const handleEventDrop = (info: any) => {
  const taskId = info.event.id
  const newDate = info.event.start.toISOString().slice(0, 10)

  // Mettre à jour la date de la tâche
  const task = tasksStore.tasks.find(t => t.id === taskId)
  if (task) {
    task.createdAt = newDate
  }
}

const handleEventClick = (info: any) => {
  const task = tasksStore.tasks.find(t => t.id === info.event.id)
  if (task) {
    selectedEvent.value = {
      id: task.id,
      title: task.title,
      start: task.createdAt,
      completed: task.completed
    }
  }
}

const closeEventModal = () => {
  selectedEvent.value = null
}

const updateEvent = () => {
  if (selectedEvent.value) {
    const task = tasksStore.tasks.find(t => t.id === selectedEvent.value!.id)
    if (task) {
      task.title = selectedEvent.value.title
      task.createdAt = selectedEvent.value.start
      task.completed = selectedEvent.value.completed
    }
    closeEventModal()
  }
}

onMounted(() => {
  // Force le rafraîchissement du calendrier après le montage
  setTimeout(() => {
    if (calendarRef.value) {
      calendarRef.value.getApi().render()
    }
  }, 100)
})
</script>

<style scoped>
.calendar-panel {
  padding: 0;
  overflow: hidden;
}

:deep(.fc) {
  font-family: inherit;
}

:deep(.fc-header-toolbar) {
  margin-bottom: 1.5rem !important;
  flex-wrap: wrap;
  gap: 0.5rem;
}

:deep(.fc-button) {
  background: var(--primary) !important;
  border: none !important;
  border-radius: 8px !important;
  padding: 8px 12px !important;
  font-size: 0.9rem !important;
  font-weight: 500 !important;
}

:deep(.fc-button:hover) {
  background: #4338ca !important;
}

:deep(.fc-button:not(:disabled).fc-button-active) {
  background: #3730a3 !important;
}

:deep(.fc-today-button) {
  background: #f8fafc !important;
  color: var(--text) !important;
  border: 1px solid var(--border) !important;
}

:deep(.fc-today-button:hover) {
  background: #e5e7eb !important;
}

:deep(.fc-daygrid-day) {
  height: 120px !important;
}

:deep(.fc-daygrid-day-number) {
  padding: 8px !important;
  font-weight: 600 !important;
}

:deep(.fc-event) {
  border-radius: 6px !important;
  border: none !important;
  font-size: 0.85rem !important;
  padding: 4px 8px !important;
  cursor: pointer !important;
}

:deep(.fc-event:hover) {
  opacity: 0.9 !important;
}

:deep(.fc-day-today) {
  background: rgba(79, 70, 229, 0.05) !important;
}

@media (max-width: 768px) {
  :deep(.fc-header-toolbar) {
    flex-direction: column;
    align-items: stretch;
  }

  :deep(.fc-toolbar-chunk) {
    display: flex;
    justify-content: center;
    margin-bottom: 0.5rem;
  }
}
</style>