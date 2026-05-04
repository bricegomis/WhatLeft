<template>
  <AdminLayout>
    <!-- Layout: Tasks List + Calendar -->
    <v-row class="mb-6">
      <!-- Left: Tasks Cards — masqué sur mobile -->
      <v-col cols="12" md="3" class="d-none d-md-flex flex-column">
        <!-- Filtre par tags -->
        <div v-if="allCalendarTags.length" class="mb-2">
          <v-autocomplete
            v-model="filterTags"
            :items="allCalendarTags"
            label="Filtrer par tags"
            multiple
            chips
            closable-chips
            clearable
            density="compact"
            variant="outlined"
            hide-details
            prepend-inner-icon="mdi-tag-outline"
            :menu-props="{ maxHeight: 200 }"
          />
        </div>
        <div class="d-flex flex-column gap-3 task-list-container">
          <div v-if="filteredUnscheduledTasks.length === 0" class="text-center text-medium-emphasis pa-8">
            <p class="text-body-2">{{ filterTags.length ? 'Aucune tâche pour ces tags' : 'Aucune tâche' }}</p>
            <p class="text-caption">{{ filterTags.length ? '' : 'Toutes les tâches sont planifiées ou créez une nouvelle tâche' }}</p>
            <v-btn v-if="filterTags.length" variant="text" size="small" @click="filterTags = []">Réinitialiser</v-btn>
          </div>
          <!-- @dragstart et @dragend gérés par FullCalendar Draggable -->
          <TaskCard
            v-for="task in filteredUnscheduledTasks"
            :key="task.id"
            :task="task"
            :draggable="true"
            @toggle-finish="tasksStore.toggleFinish(task.id)"
            @delete="tasksStore.removeTask(task.id)"
            @click="openEditDialog(task)"
          />
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
import { ref, computed, reactive, onMounted, nextTick } from 'vue'
import { useDisplay } from 'vuetify'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid'
import interactionPlugin from '@fullcalendar/interaction'
import { Draggable } from '@fullcalendar/interaction'
import VueCalendar from '@fullcalendar/vue3'
import { useTasksStore } from '../stores/tasks'
import type { Task } from '../stores/tasks'
import AdminLayout from '../layouts/AdminLayout.vue'
import TaskCard from '../components/TaskCard.vue'

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
const filterTags = ref<string[]>([])

const allCalendarTags = computed(() =>
  [...new Set(tasksStore.unscheduledTasks.flatMap(t => t.tags))].sort()
)

const filteredUnscheduledTasks = computed(() => {
  if (filterTags.value.length === 0) return tasksStore.unscheduledTasks
  return tasksStore.unscheduledTasks.filter(t =>
    filterTags.value.every(tag => t.tags.includes(tag))
  )
})
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
  events: tasksStore.scheduledTasks.map(task => {
    const startDate = new Date(task.startAt as string)
    const endDate = new Date(startDate.getTime() + task.duration * 60 * 1000)
    return {
      id: task.id,
      title: task.title,
      start: task.startAt as string,
      end: endDate.toISOString(),
      extendedProps: { finishAt: task.finishAt, duration: task.duration },
      backgroundColor: task.finishAt ? '#4caf50' : '#4f46e5',
      borderColor: task.finishAt ? '#4caf50' : '#4f46e5'
    }
  }),
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
    // Retirer l'événement local de FC : le store va se mettre à jour,
    // calendarOptions recalcule, et FC affiche l'événement depuis le store.
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
  if (task) openEditDialog(task)
}

const openEditDialog = (task: Task) => {
  selectedEvent.id = task.id
  selectedEvent.title = task.title
  selectedEvent.start = task.startAt || ''
  selectedEvent.finishAt = task.finishAt || ''
  showEventModal.value = true
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
.task-list-container {
  overflow-y: auto;
}
</style>