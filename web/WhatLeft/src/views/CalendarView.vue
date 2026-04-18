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
        <div class="d-flex flex-column gap-3 task-list-container">
          <div v-if="tasksStore.tasks.length === 0" class="text-center text-medium-emphasis pa-8">
            <p class="text-body-2">Aucune tâche</p>
            <p class="text-caption">Créez une tâche à partir de la page des tâches</p>
          </div>
          <!-- @dragstart="handleDragStart($event, task)"
            @dragend="handleDragEnd" -->
          <v-card
            v-for="task in tasksStore.tasks"
            :key="task.id"
            class="draggable-task fc-event"
            :data-event-id="task.id"
            :data-duration="task.duration"
            :style="{ 
              minHeight: `${task.duration >= 4 ? 100 : 80}px`,
              background: getTaskBackground(task)
            }"
            :class="{ completed: task.finishAt }"
          >
            <v-card-text class="d-flex flex-column justify-space-between h-100 pa-3 position-relative">
              <!-- Title and Duration Row -->
              <div class="d-flex justify-space-between align-start gap-2">
                <h4 class="text-body-2 font-weight-bold text-white flex-grow-1" :class="{ 'text-decoration-line-through': task.finishAt }">
                  {{ task.title }}
                </h4>
                <span class="text-caption text-white font-weight-bold flex-shrink-0">
                  {{ task.duration }}h
                </span>
              </div>
              
              <!-- Time info -->
              <div class="text-caption text-white-50 mt-1">
                <p class="mb-0">{{ getTaskTimeRange(task) }}</p>
              </div>
              
              <!-- Tags Row -->
              <div class="d-flex flex-wrap gap-1 mt-2">
                <v-chip
                  v-for="(tag, index) in getTaskTags(task)"
                  :key="index"
                  size="x-small"
                  variant="outlined"
                  class="tag-chip"
                >
                  {{ tag }}
                </v-chip>
                <span v-if="getTaskTags(task).length === 0" class="text-caption text-white-50 align-self-center">
                  Pas de tags
                </span>
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
import { ref, computed, reactive, onMounted, nextTick } from 'vue'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid'
import interactionPlugin from '@fullcalendar/interaction'
import { Draggable } from '@fullcalendar/interaction'
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
  plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
  initialView: 'timeGridDay',
  editable: true,
  droppable: true,
  headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'dayGridMonth,timeGridWeek,timeGridDay'
    },
  locale: 'fr',
  firstDay: 1,
  drop: (info: any) => {
    // Handle drop from external elements
    console.log('Event dropped:', info)
  },
  eventReceive: (info: any) => {
    // Handle event received from external drag
    // The ID is stored in the element's data attribute
    const draggedEl = info.draggedEl
    const taskId = draggedEl?.getAttribute('data-event-id')
    const newDate = info.event.start?.toISOString().split('T')[0]
    
    if (taskId && newDate) {
      tasksStore.updateTask(taskId, { createdAt: newDate })
      console.log(`Tâche ${taskId} déplacée au ${newDate}`)
    }
  }
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
  console.log("on drop");
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

const getTaskTags = (task: any) => {
  // Exemple: extraire les tags du titre s'ils sont entre #
  // Sinon, retourner des tags basés sur la catégorie
  const titleTags = (task.title.match(/#[\w-]+/g) || []).map(tag => tag.substring(1))
  
  if (titleTags.length > 0) {
    return titleTags.slice(0, 3) // Max 3 tags
  }
  
  // Tags basés sur la durée
  if (task.duration >= 8) {
    return ['long', 'important']
  } else if (task.duration >= 4) {
    return ['moyen']
  } else {
    return ['rapide']
  }
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('fr-FR', {
    month: 'short',
    day: 'numeric'
  })
}

const getTaskBackground = (task: any) => {
  // Gradient based on duration
  if (task.finishAt) {
    return 'linear-gradient(135deg, #4caf50 0%, #45a049 100%)'
  }
  
  // Color intensity based on duration
  if (task.duration >= 8) {
    return 'linear-gradient(135deg, #ff5252 0%, #ff1744 100%)'
  } else if (task.duration >= 4) {
    return 'linear-gradient(135deg, #ff9800 0%, #f57c00 100%)'
  } else {
    return 'linear-gradient(135deg, #4f46e5 0%, #4338ca 100%)'
  }
}

const getTaskTimeRange = (task: any) => {
  if (!task.createdAt) return ''
  
  const startTime = new Date(task.createdAt)
  const endTime = new Date(startTime.getTime() + task.duration * 60 * 60 * 1000)
  
  const start = startTime.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
  const end = endTime.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
  
  return `${start} - ${end}`
}

// Drag & drop is now handled by FullCalendar's Draggable

onMounted(async () => {
  // Wait for DOM to be ready
  await nextTick()
  
  // Initialize Draggable for external events
  const containerEl = document.querySelector('.task-list-container')
  if (containerEl) {
    new Draggable(containerEl, {
      itemSelector: '.fc-event',
      eventData: function(eventEl: HTMLElement) {
        return {
          title: eventEl.textContent?.split('\n')[0] || 'Tâche',
          duration: parseInt(eventEl.getAttribute('data-duration') || '1')
        }
      }
    })
  }

  // Initialize calendar
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
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: none;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  border-radius: 12px;
  overflow: hidden;
  position: relative;
  user-select: none;
}

.draggable-task:hover {
  cursor: grab;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.25);
  transform: translateY(-4px);
}

.draggable-task:active {
  cursor: grabbing;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.3);
  transform: translateY(-6px);
}

.draggable-task.completed {
  opacity: 0.85;
}

.draggable-task .task-actions {
  display: flex;
  gap: 4px;
  margin-top: 8px;
  opacity: 0.8;
  transition: opacity 0.2s ease;
  align-self: flex-end;
}

.draggable-task:hover .task-actions {
  opacity: 1;
}

.tag-chip {
  border-color: rgba(255, 255, 255, 0.5) !important;
  color: white !important;
  font-weight: 500;
  height: 20px !important;
}

.tag-chip :deep(.v-chip__content) {
  font-size: 0.75rem !important;
  padding: 0 6px !important;
}

.position-relative {
  position: relative;
}
</style>