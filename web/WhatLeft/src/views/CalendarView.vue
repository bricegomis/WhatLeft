<template>
  <AdminLayout>
    <!-- Page Header -->
    <v-row class="mb-6">
      <v-col cols="12">
        <div>
          <h1 class="text-h4 font-weight-bold mb-2">Calendrier</h1>
          <p class="text-body-1 text-medium-emphasis mb-0">
            Glissez les tâches à gauche sur le calendrier pour les planifier.
          </p>
        </div>
      </v-col>
    </v-row>

    <!-- Layout: Tasks List + Calendar -->
    <v-row class="mb-6">
      <!-- Left: Tasks Cards -->
      <v-col cols="12" md="3">
        <div class="d-flex flex-column gap-3">
          <div v-if="tasksStore.tasks.length === 0" class="text-center text-medium-emphasis pa-8">
            <p class="text-body-2">Aucune tâche</p>
            <p class="text-caption">Créez une tâche à partir de la page des tâches</p>
          </div>
          
          <v-card
            v-for="task in tasksStore.tasks"
            :key="task.id"
            draggable
            @dragstart="handleDragStart($event, task)"
            @dragend="handleDragEnd"
            class="draggable-task cursor-grab"
            :style="{ minHeight: `${60 + task.duration * 20}px` }"
            :class="{ completed: task.finishAt, 'opacity-75': task.finishAt }"
          >
            <v-card-text class="d-flex flex-column justify-space-between h-100 pa-3">
              <div>
                <h4 class="text-body-2 font-weight-bold mb-2" :class="{ 'text-decoration-line-through': task.finishAt }">
                  {{ task.title }}
                </h4>
                <div class="text-caption text-medium-emphasis">
                  <p class="mb-1">⏱ {{ task.duration }}h</p>
                  <p v-if="task.createdAt" class="mb-0">
                    {{ formatDate(task.createdAt) }}
                  </p>
                </div>
              </div>
              
              <div class="d-flex gap-2 mt-2">
                <v-btn
                  size="x-small"
                  variant="outlined"
                  color="primary"
                  @click="toggleFinish(task.id)"
                  :icon="task.finishAt ? 'mdi-undo' : 'mdi-check'"
                  class="flex-grow-1"
                />
                <v-btn
                  size="x-small"
                  variant="outlined"
                  color="error"
                  icon="mdi-delete"
                  @click="removeTask(task.id)"
                />
              </div>
            </v-card-text>
          </v-card>
        </div>
      </v-col>

      <!-- Right: Calendar -->
      <v-col cols="12" md="9">
        <v-card class="h-100">
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
      </v-col>
    </v-row>

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

const toggleFinish = async (id: string) => {
  const task = tasksStore.tasks.find(t => t.id === id)
  if (task) {
    await tasksStore.updateTask(id, {
      finishAt: task.finishAt ? null : new Date().toISOString().split('T')[0]
    })
  }
}

const removeTask = async (id: string) => {
  if (confirm('Êtes-vous sûr de vouloir supprimer cette tâche ?')) {
    await tasksStore.removeTask(id)
  }
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('fr-FR', {
    month: 'short',
    day: 'numeric'
  })
}

const handleDragStart = (event: DragEvent, task: any) => {
  if (event.dataTransfer) {
    event.dataTransfer.effectAllowed = 'move'
    event.dataTransfer.setData('application/json', JSON.stringify({
      id: task.id,
      title: task.title,
      duration: 3600000 // 1 hour in milliseconds
    }))
  }
}

const handleDragEnd = () => {
  // Optional: handle drag end state if needed
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

<style scoped>
.draggable-task {
  cursor: grab;
  transition: all 0.2s ease;
  border: 2px solid rgba(0, 0, 0, 0.05);
}

.draggable-task:hover {
  cursor: grab;
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.12);
  transform: translateY(-2px);
}

.draggable-task:active {
  cursor: grabbing;
}

.draggable-task.completed {
  opacity: 0.6;
  background-color: rgba(76, 175, 80, 0.05);
}

.cursor-grab {
  cursor: grab;
}

.cursor-grab:active {
  cursor: grabbing;
}
</style>