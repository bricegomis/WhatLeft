<template>
  <AdminLayout>
    <!-- Layout: Tasks List + Calendar -->
    <v-row class="mb-6">
      <!-- Left: Tasks Cards — masqué sur mobile -->
      <v-col cols="12" md="3" class="d-none d-md-flex flex-column">
        <div class="d-flex flex-column gap-3 task-list-container">
          <div v-if="tasksStore.unscheduledTasks.length === 0" class="text-center text-medium-emphasis pa-8">
            <p class="text-body-2">Aucune tâche</p>
            <p class="text-caption">Toutes les tâches sont planifiées ou créez une nouvelle tâche</p>
          </div>
          <!-- @dragstart="handleDragStart($event, task)"
            @dragend="handleDragEnd" -->
          <v-card
            v-for="task in tasksStore.unscheduledTasks"
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
                  {{ task.duration }} min
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

      <!-- Right: Calendar — plein écran sur mobile -->
      <v-col cols="12" md="9">
        <v-card class="h-100">
          <v-card-text class="pa-0">
            <VueCalendar
              ref="calendarRef"
              :options="calendarOptions"
              @eventDrop="handleEventDrop"
              @eventClick="handleEventClick"
            />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Edit Event Dialog -->
    <v-dialog
      v-model="showEventModal"
      max-width="500"
      :fullscreen="$vuetify.display.xs"
    >
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
            label="Date de début prévue"
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
import { ref, computed, reactive, onMounted, nextTick, watch } from 'vue'
import { useDisplay } from 'vuetify'
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
const { mobile } = useDisplay()
const calendarRef = ref()
const showEventModal = ref(false)
const selectedEvent = reactive<SelectedEvent>({
  id: '',
  title: '',
  start: '',
  finishAt: ''
})

const calendarOptions = computed(() => ({
  plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
  // Sur mobile : vue jour (lisible), sur desktop : vue semaine
  initialView: mobile.value ? 'timeGridDay' : 'timeGridWeek',
  editable: true,
  droppable: true,
  height: mobile.value ? 600 : 'auto',
  events: (_info: any, successCallback: any) => {
    successCallback(
      tasksStore.scheduledTasks.map(task => {
        const startDate = new Date(task.startAt as string)
        const endDate = new Date(startDate.getTime() + task.duration * 60 * 1000)
        return {
          id: task.id,
          title: task.title,
          start: task.startAt as string,
          end: endDate.toISOString(),
          extendedProps: {
            finishAt: task.finishAt,
            duration: task.duration
          },
          backgroundColor: task.finishAt ? '#4caf50' : '#4f46e5',
          borderColor: task.finishAt ? '#4caf50' : '#4f46e5'
        }
      })
    )
  },
  headerToolbar: {
      left: 'prev,next today',
      center: mobile.value ? '' : 'title',
      right: 'timeGridDay,dayGridMonth,timeGridWeek'
    },
  locale: 'fr',
  firstDay: 1,
  drop: (info: any) => {
    console.log('Event dropped:', info)
  },
  eventReceive: (info: any) => {
    const taskId = info.event.id
    const newStartAt = info.event.start?.toISOString()

    // Supprimer l'événement local ajouté par FullCalendar :
    // le watch sur scheduledTasks va déclencher refetchEvents qui le recharge proprement.
    // Sans ce remove(), on obtient un doublon (local + rechargé depuis le store).
    info.event.remove()

    if (taskId && newStartAt) {
      tasksStore.updateTask(taskId, { startAt: newStartAt })
    }
  }
}))

const handleEventDrop = async (info: any) => {
  const taskId = info.event.id
  const newStartAt = info.event.start.toISOString()
  console.log("on drop");
  try {
    await tasksStore.updateTask(taskId, { startAt: newStartAt })
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
    selectedEvent.start = task.startAt || ''
    selectedEvent.finishAt = task.finishAt || ''
    showEventModal.value = true
  }
}

const updateEvent = async () => {
  try {
    await tasksStore.updateTask(selectedEvent.id, {
      title: selectedEvent.title,
      startAt: selectedEvent.start || null,
      finishAt: selectedEvent.finishAt || null
    })
    showEventModal.value = false
  } catch (error) {
    console.error('Erreur lors de la mise à jour:', error)
  }
}

const getTaskTags = (task: any) => {
  return task.tags || []
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
  if (!task.startAt) return 'Non planifiée'
  
  const startTime = new Date(task.startAt)
  const endTime = new Date(startTime.getTime() + task.duration * 60 * 60 * 1000)
  
  const dateStr = startTime.toLocaleDateString('fr-FR', { month: 'short', day: 'numeric' })
  const startStr = startTime.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
  const endStr = endTime.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
  
  return `${dateStr} • ${startStr} - ${endStr}`
}

// Drag & drop is now handled by FullCalendar's Draggable

// Sync calendar when tasks change (function source needs explicit refetch)
watch(
  () => tasksStore.scheduledTasks,
  async () => {
    await nextTick()
    calendarRef.value?.getApi()?.refetchEvents()
  },
  { deep: true }
)

onMounted(async () => {
  // Charger les tâches en premier
  if (tasksStore.tasks.length === 0) {
    await tasksStore.fetchTasks()
  }

  // Wait for DOM to be ready
  await nextTick()
  
  // Initialize Draggable for external events
  const containerEl = document.querySelector('.task-list-container') as HTMLElement | null
  if (containerEl) {
    new Draggable(containerEl, {
      itemSelector: '.fc-event',
      eventData: function(eventEl: HTMLElement) {
        // Get the task ID from the element or its parent
        let element = eventEl
        let taskId = element.getAttribute('data-event-id')
        
        // If not found, search in parent elements
        if (!taskId) {
          element = eventEl.closest('[data-event-id]') as HTMLElement
          if (element) {
            taskId = element.getAttribute('data-event-id')
          }
        }
        
        // Find the task in the store to get its duration
        let durationHours = 1
        if (taskId) {
          const task = tasksStore.tasks.find(t => t.id === taskId)
          if (task) {
            durationHours = task.duration
          }
        }
        
        // Extract title (first line of text)
        const title = eventEl.textContent?.split('\n')[0]?.trim() || 'Tâche'
        
        return {
          id: taskId ?? undefined,
          title: title,
          duration: `PT${durationHours}H` // ISO 8601 format
        }
      }
    })
  }

  // Initialize calendar
  setTimeout(() => {
    if (calendarRef.value) {
      calendarRef.value.getApi().render()
      calendarRef.value.getApi().refetchEvents()
    }
  }, 100)
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