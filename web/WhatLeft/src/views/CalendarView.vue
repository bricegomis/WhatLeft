<template>
  <AdminLayout>
    <!-- Page Header -->
    <v-row class="mb-6">
      <v-col cols="12">
        <div>
          <h1 class="text-h4 font-weight-bold mb-2">Calendrier</h1>
        </div>
      </v-col>
    </v-row>

    <!-- Calendar -->
    <v-card class="mb-6">
      <v-card-text class="pa-0">
        <VueCalendar
          ref="calendarRef"
          :options="calendarOptions"
          :events="calendarEvents"
          @eventDrop="handleEventDrop"
          @eventClick="handleEventClick"
        />
      </v-card-text>
    </v-card>

    <!-- Edit Event Dialog -->
    <v-dialog v-model="showEventModal" max-width="500">
      <v-card>
        <v-card-title>Modifier la tâche</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="selectedEvent.title"
            label="Titre"
            class="mb-4"
          />

          <v-text-field
            v-model="selectedEvent.start"
            label="Date de création"
            type="date"
            class="mb-4"
          />

          <v-text-field
            v-model="selectedEvent.finishAt"
            label="Date de fin"
            type="date"
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="showEventModal = false">Annuler</v-btn>
          <v-btn color="primary" @click="updateEvent">Sauvegarder</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from 'vue'
import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'
import bootstrap5Plugin from '@fullcalendar/bootstrap5'
import VueCalendar from '@fullcalendar/vue3'
import { useTasksStore } from '../stores/tasks'
import AdminLayout from '../layouts/AdminLayout.vue'

interface SelectedEvent {
  id: string
  title: string
  start: string
  finishAt: string
}

const tasksStore = useTasksStore()
const calendarRef = ref()
const showEventModal = ref(false)
const selectedEvent = reactive<SelectedEvent>({
  id: '',
  title: '',
  start: '',
  finishAt: ''
})

const calendarOptions = {
  plugins: [dayGridPlugin, interactionPlugin, bootstrap5Plugin],
  themeSystem: 'bootstrap5',
  initialView: 'dayGridMonth',
  editable: true,
  droppable: true,
  headerToolbar: {
    left: 'prev,next today',
    center: 'title',
    right: 'dayGridMonth,dayGridWeek'
  },
  locale: 'fr',
  firstDay: 1,
  height: 'auto',
  eventDisplay: 'block'
}

const calendarEvents = computed(() => {
  return tasksStore.tasks.map(task => ({
    id: task.id,
    title: task.title,
    start: task.createdAt,
    extendedProps: {
      finishAt: task.finishAt
    },
    backgroundColor: task.finishAt ? '#4caf50' : '#4f46e5',
    borderColor: task.finishAt ? '#4caf50' : '#4f46e5'
  }))
})

const handleEventDrop = async (info: any) => {
  const taskId = info.event.id
  const newDate = info.event.start.toISOString().slice(0, 10)

  try {
    await tasksStore.updateTask(taskId, { createdAt: newDate })
  } catch (error) {
    info.revert()
    console.error('Erreur lors du déplacement de la tâche:', error)
  }
}

const handleEventClick = (info: any) => {
  const task = tasksStore.tasks.find(t => t.id === info.event.id)
  if (task) {
    selectedEvent.id = task.id
    selectedEvent.title = task.title
    selectedEvent.start = task.createdAt
    selectedEvent.finishAt = task.finishAt || ''
    showEventModal.value = true
  }
}

const updateEvent = async () => {
  try {
    await tasksStore.updateTask(selectedEvent.id, {
      title: selectedEvent.title,
      createdAt: selectedEvent.start,
      finishAt: selectedEvent.finishAt || null
    })
    showEventModal.value = false
  } catch (error) {
    console.error('Erreur lors de la mise à jour:', error)
  }
}

onMounted(() => {
  setTimeout(() => {
    if (calendarRef.value) {
      calendarRef.value.getApi().render()
    }
  }, 100)

  if (tasksStore.tasks.length === 0) {
    tasksStore.fetchTasks()
  }
})
</script>