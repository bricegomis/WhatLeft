<template>
  <AdminLayout>
    <template #actions>
      <v-btn-toggle
        v-model="viewMode"
        mandatory
        density="compact"
        variant="outlined"
        divided
        class="me-2"
      >
        <v-btn value="table" size="small" icon="mdi-table" />
        <v-btn value="cards" size="small" icon="mdi-view-grid" />
      </v-btn-toggle>
    </template>

    <!-- Error Messages -->
    <v-alert
      v-if="hasError"
      type="error"
      class="mb-6"
      dismissible
      @click:close="clearError"
    >
      {{ error }}
      <v-btn size="small" variant="text" @click="retryLoad" class="ms-2">
        Réessayer
      </v-btn>
    </v-alert>

    <!-- Loading State -->
    <div v-if="isLoading && tasks.length === 0" class="text-center pa-12">
      <v-progress-circular indeterminate color="primary" class="mb-4" />
      <p class="text-body-1">Chargement des tâches...</p>
    </div>

    <!-- Tasks Table -->
    <v-card v-else-if="tasks.length > 0 && viewMode === 'table'" class="mb-6">
      <v-data-table
        :headers="tableHeaders"
        :items="tasks"
        :loading="isLoading"
        item-key="id"
      >
        <template #item.title="{ item }">
          <div class="d-flex align-center gap-2 cursor-pointer" @click="openEditDialog(item)">
            <v-checkbox
              :checked="!!item.finishAt"
              @click.stop
              @change="toggleFinish(item.id)"
              hide-details
              :disabled="isLoading"
            />
            <span :class="{ 'text-decoration-line-through': item.finishAt }">
              {{ item.title }}
            </span>
          </div>
        </template>

        <template #item.createdAt="{ item }">
          {{ formatDate(item.createdAt) }}
        </template>

        <template #item.tags="{ item }">
          <div class="d-flex flex-wrap gap-1 py-1">
            <v-chip
              v-for="tag in item.tags"
              :key="tag"
              size="x-small"
              variant="tonal"
              color="primary"
            >{{ tag }}</v-chip>
            <span v-if="item.tags.length === 0" class="text-medium-emphasis text-caption">—</span>
          </div>
        </template>
        <template #item.actions="{ item }">
          <v-btn
            v-if="!item.finishAt"
            size="small"
            variant="outlined"
            color="success"
            @click="toggleFinish(item.id)"
            :disabled="isLoading"
            class="me-2"
          >
            Terminer
          </v-btn>
          <v-btn
            v-else
            size="small"
            variant="outlined"
            color="primary"
            @click="toggleFinish(item.id)"
            :disabled="isLoading"
            class="me-2"
          >
            Reprendre
          </v-btn>
          <v-btn
            size="small"
            variant="outlined"
            color="error"
            @click="removeTask(item.id)"
            :disabled="isLoading"
            icon="mdi-delete"
          />
        </template>
      </v-data-table>
    </v-card>

    <!-- Tasks List (style iOS Rappels) -->
    <v-card v-else-if="tasks.length > 0 && viewMode === 'cards'" class="mb-6" style="overflow:hidden;">
      <template v-for="(task, index) in tasks" :key="task.id">
        <div style="position:relative; overflow:hidden;">

          <!-- Actions révélées au glissement gauche -->
          <div style="position:absolute; right:0; top:0; bottom:0; display:flex; width:210px;">
            <button
              style="flex:1; background:#34c759; color:white; border:none; cursor:pointer; display:flex; flex-direction:column; align-items:center; justify-content:center; gap:2px;"
              @click="toggleFinish(task.id); closeSwipe(task.id)"
            >
              <v-icon size="20">mdi-check</v-icon>
              <span style="font-size:10px;">Terminer</span>
            </button>
            <button
              style="flex:1; background:#ff9500; color:white; border:none; cursor:pointer; display:flex; flex-direction:column; align-items:center; justify-content:center; gap:2px;"
              @click="scheduleTonight(task.id)"
            >
              <v-icon size="20">mdi-weather-sunset</v-icon>
              <span style="font-size:10px;">Ce soir</span>
            </button>
            <button
              style="flex:1; background:#007aff; color:white; border:none; cursor:pointer; display:flex; flex-direction:column; align-items:center; justify-content:center; gap:2px;"
              @click="scheduleTomorrowMorning(task.id)"
            >
              <v-icon size="20">mdi-weather-sunrise</v-icon>
              <span style="font-size:10px;">Demain</span>
            </button>
          </div>

          <!-- Contenu principal (glisse à gauche) -->
          <div
            class="d-flex align-center px-4 py-3"
            :style="{
              background: 'white',
              position: 'relative',
              zIndex: 1,
              transform: `translateX(${getSwipeOffset(task.id)}px)`,
              transition: isSwipeDragging(task.id) ? 'none' : 'transform 0.25s ease',
            }"
            @touchstart="onTouchStart(task.id, $event)"
            @touchmove="onTouchMove(task.id, $event)"
            @touchend="onTouchEnd(task.id, $event)"
            @click="getSwipeOffset(task.id) !== 0 ? closeSwipe(task.id) : openEditDialog(task)"
          >
            <!-- Cercle checkbox -->
            <v-btn
              :icon="task.finishAt ? 'mdi-check-circle' : 'mdi-circle-outline'"
              :color="task.finishAt ? 'success' : 'grey-lighten-1'"
              variant="plain"
              size="small"
              @click.stop="toggleFinish(task.id)"
              :disabled="isLoading"
              class="mr-3 flex-shrink-0"
            />

            <!-- Texte -->
            <div class="flex-grow-1" style="min-width:0; position:relative;">
              <span style="position:absolute; top:0; right:0; font-size:10px; color:#999; white-space:nowrap;">{{ task.duration }} min</span>
              <div :class="['text-body-1', { 'text-decoration-line-through text-medium-emphasis': task.finishAt }]">
                {{ task.title }}
              </div>
              <div v-if="task.startAt || task.tags.length" class="d-flex align-center flex-wrap gap-1 mt-1">
                <span v-if="task.startAt" class="text-caption text-primary d-flex align-center ga-1">
                  <v-icon size="12">mdi-clock-outline</v-icon>{{ formatDateTime(task.startAt) }}
                </span>
                <v-chip v-for="tag in task.tags" :key="tag" size="x-small" variant="tonal" color="primary">{{ tag }}</v-chip>
              </div>
              <div v-if="taskRecurrenceType(task)" style="position:absolute; bottom:0; right:0;">
                <v-tooltip :text="taskRecurrenceType(task) === 'Daily' ? 'Journalier' : 'Hebdomadaire'" location="start">
                  <template #activator="{ props }">
                    <v-icon v-bind="props" size="14" color="deep-purple-lighten-2">
                      {{ taskRecurrenceType(task) === 'Daily' ? 'mdi-calendar-today' : taskRecurrenceType(task) === 'Monthly' ? 'mdi-calendar-month' : 'mdi-calendar-week' }}
                    </v-icon>
                  </template>
                </v-tooltip>
              </div>
            </div>

            <!-- Bouton supprimer (desktop uniquement, sur hover) -->
            <v-btn
              class="d-none d-sm-flex flex-shrink-0 ml-2"
              icon="mdi-delete-outline"
              variant="plain"
              color="grey"
              size="small"
              @click.stop="removeTask(task.id)"
              :disabled="isLoading"
            />
          </div>
        </div>
        <v-divider v-if="index < tasks.length - 1" />
      </template>
    </v-card>

    <!-- Empty State -->
    <v-card v-else class="text-center pa-12">
      <v-icon color="primary" size="64" class="mb-4">mdi-checkbox-marked-outline</v-icon>
      <h2 class="text-h6 mb-2">Aucune tâche</h2>
      <p class="text-body-2 text-medium-emphasis mb-6">
        Il n'y a pas encore de tâches. Créez votre première tâche !
      </p>
      <v-btn
        color="primary"
        prepend-icon="mdi-plus"
        @click="openModal"
        :disabled="isLoading"
      >
        Créer une tâche
      </v-btn>
    </v-card>

    <!-- Edit Task Dialog -->
    <v-dialog
      v-model="editDialog"
      max-width="500"
      :fullscreen="$vuetify.display.xs"
    >
      <v-card v-if="editingTask">
        <v-card-title class="d-flex align-center">
          <span class="flex-grow-1">Modifier la tâche</span>
          <v-btn
            v-if="editingTask.recurringTaskTemplateId"
            size="small"
            variant="tonal"
            color="primary"
            prepend-icon="mdi-repeat"
            @click="goToTemplate(editingTask.recurringTaskTemplateId!)"
          >
            Récurrence
          </v-btn>
        </v-card-title>

        <v-card-text>
          <v-text-field
            v-model="editTitle"
            label="Titre"
            :disabled="isSavingEdit"
            required
            class="mb-3"
          />
          <v-text-field
            v-model.number="editDuration"
            label="Durée (minutes)"
            type="number"
            min="1"
            step="5"
            :disabled="isSavingEdit"
            class="mb-3"
          />
          <v-text-field
            v-model="editStartAt"
            label="Planifié le"
            type="datetime-local"
            :disabled="isSavingEdit"
            class="mb-3"
          />
          <v-combobox
            v-model="editTags"
            :items="allExistingTags"
            label="Tags"
            multiple
            chips
            closable-chips
            clearable
            :disabled="isSavingEdit"
          />
        </v-card-text>

        <v-card-actions>
          <v-btn
            color="error"
            variant="text"
            prepend-icon="mdi-delete-outline"
            :disabled="isSavingEdit"
            @click="editDialogDelete"
          >
            Supprimer
          </v-btn>
          <v-spacer />
          <v-btn variant="text" @click="closeEditDialog" :disabled="isSavingEdit">Annuler</v-btn>
          <v-btn
            v-if="!editingTask.finishAt"
            color="success"
            variant="tonal"
            prepend-icon="mdi-check"
            :disabled="isSavingEdit"
            @click="editDialogFinish"
          >
            Terminer
          </v-btn>
          <v-btn
            color="primary"
            :disabled="!editTitle.trim() || isSavingEdit"
            :loading="isSavingEdit"
            @click="saveEdit"
          >
            Enregistrer
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Create Task Dialog -->
    <v-dialog
      v-model="isModalOpen"
      max-width="500"
      :fullscreen="$vuetify.display.xs"
    >
      <v-card>
        <v-card-title>Ajouter une tâche</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="newTaskTitle"
            label="Titre de la tâche"
            placeholder="Par exemple : Envoyer le rapport"
            :disabled="isCreating"
            required
            class="mb-4"
          />

          <v-text-field
            v-model.number="newTaskDuration"
            label="Durée (minutes)"
            type="number"
            min="1"
            step="5"
            :disabled="isCreating"
            class="mb-4"
          />

          <v-text-field
            v-model="newTaskFinishAt"
            label="Terminé le (optionnel)"
            type="date"
            :disabled="isCreating"
            class="mb-4"
          />

          <v-combobox
            v-model="newTaskTags"
            :items="allExistingTags"
            label="Tags"
            placeholder="Sélectionner ou créer des tags…"
            multiple
            chips
            closable-chips
            clearable
            :disabled="isCreating"
            hint="Appuyer sur Entrée pour créer un nouveau tag"
            persistent-hint
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="closeModal" :disabled="isCreating">
            Annuler
          </v-btn>
          <v-btn
            color="primary"
            @click="createTask"
            :disabled="!newTaskTitle.trim() || isCreating"
            :loading="isCreating"
          >
            Créer
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <!-- FAB ajout tâche (toujours visible) -->
    <v-btn
      color="primary"
      icon
      size="large"
      style="position: fixed; bottom: 28px; right: 28px; z-index: 1000;"
      :disabled="isLoading"
      @click="openModal"
      elevation="4"
    >
      <v-icon size="28">mdi-plus</v-icon>
      <v-tooltip activator="parent" location="start">Nouvelle tâche</v-tooltip>
    </v-btn>

  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useDisplay } from 'vuetify'
import { storeToRefs } from 'pinia'
import { useTasksStore } from '../stores/tasks'
import { useRecurringStore } from '../stores/recurring'
import AdminLayout from '../layouts/AdminLayout.vue'
import type { Task } from '../stores/tasks'

const tasksStore = useTasksStore()
const { tasks, isLoading, hasError, error } = storeToRefs(tasksStore)

const recurringStore = useRecurringStore()
const { templates: recurringTemplates } = storeToRefs(recurringStore)

function taskRecurrenceType(task: Task): 'Daily' | 'Weekly' | 'Monthly' | null {
  if (!task.recurringTaskTemplateId) return null
  return recurringTemplates.value.find(t => t.id === task.recurringTaskTemplateId)?.recurrenceType ?? null
}
const { mobile } = useDisplay()
const router = useRouter()

// ── Dialog de détail/édition ────────────────────────────────────────────────
const editDialog = ref(false)
const editingTask = ref<Task | null>(null)
const editTitle = ref('')
const editDuration = ref(1)
const editStartAt = ref('')
const editTags = ref<string[]>([])
const isSavingEdit = ref(false)

function openEditDialog(task: Task) {
  if (getSwipeOffset(task.id) !== 0) { closeSwipe(task.id); return }
  editingTask.value = task
  editTitle.value = task.title
  editDuration.value = task.duration
  editStartAt.value = task.startAt ? new Date(task.startAt).toISOString().slice(0, 16) : ''
  editTags.value = [...task.tags]
  editDialog.value = true
}

function closeEditDialog() {
  editDialog.value = false
  editingTask.value = null
}

async function saveEdit() {
  if (!editingTask.value || !editTitle.value.trim()) return
  isSavingEdit.value = true
  try {
    await tasksStore.updateTask(editingTask.value.id, {
      title: editTitle.value,
      duration: editDuration.value,
      startAt: editStartAt.value ? new Date(editStartAt.value).toISOString() : null,
      tags: editTags.value
    })
    closeEditDialog()
  } finally {
    isSavingEdit.value = false
  }
}

async function editDialogFinish() {
  if (!editingTask.value) return
  closeEditDialog()
  await tasksStore.toggleFinish(editingTask.value.id)
}

async function editDialogDelete() {
  if (!editingTask.value) return
  const id = editingTask.value.id
  closeEditDialog()
  await tasksStore.removeTask(id)
}

function goToTemplate(templateId: string) {
  closeEditDialog()
  router.push({ path: '/recurring', query: { highlight: templateId } })
}

// ── Mode d'affichage ─────────────────────────────────────────────────────────
const STORAGE_KEY = 'whatleft.tasks.viewMode'
const viewMode = ref<'table' | 'cards'>(
  (localStorage.getItem(STORAGE_KEY) as 'table' | 'cards') ?? (mobile.value ? 'cards' : 'table')
)
watch(viewMode, (v) => localStorage.setItem(STORAGE_KEY, v))

// ── Swipe (vue liste) ───────────────────────────────────────────────────────
const ACTION_WIDTH = 210
interface SwipeItem {
  offset: number; startX: number; startY: number
  baseOffset: number; dragging: boolean; lockAxis: 'h' | 'v' | null
}
const swipeMap = reactive<Record<string, SwipeItem>>({})

function ensureSwipe(id: string): SwipeItem {
  if (!swipeMap[id]) swipeMap[id] = { offset: 0, startX: 0, startY: 0, baseOffset: 0, dragging: false, lockAxis: null }
  return swipeMap[id]
}
function getSwipeOffset(id: string) { return swipeMap[id]?.offset ?? 0 }
function isSwipeDragging(id: string) { return swipeMap[id]?.dragging ?? false }

function onTouchStart(taskId: string, e: TouchEvent) {
  for (const id in swipeMap) { if (id !== taskId) swipeMap[id].offset = 0 }
  const s = ensureSwipe(taskId)
  s.startX = e.touches[0].clientX; s.startY = e.touches[0].clientY
  s.baseOffset = s.offset; s.dragging = true; s.lockAxis = null
}
function onTouchMove(taskId: string, e: TouchEvent) {
  const s = swipeMap[taskId]
  if (!s?.dragging) return
  const dx = e.touches[0].clientX - s.startX
  const dy = e.touches[0].clientY - s.startY
  if (s.lockAxis === null) {
    if (Math.abs(dx) > 8 || Math.abs(dy) > 8) s.lockAxis = Math.abs(dx) >= Math.abs(dy) ? 'h' : 'v'
    return
  }
  if (s.lockAxis === 'h') {
    e.preventDefault()
    s.offset = Math.max(-ACTION_WIDTH, Math.min(0, s.baseOffset + dx))
  }
}
function onTouchEnd(taskId: string, e: TouchEvent) {
  const s = swipeMap[taskId]
  if (!s) return
  if (s.lockAxis === 'h') {
    const dx = e.changedTouches[0].clientX - s.startX
    if (dx < -60) s.offset = -ACTION_WIDTH
    else if (dx > 30) s.offset = 0
    else s.offset = s.baseOffset
  }
  s.dragging = false; s.lockAxis = null
}
function closeSwipe(taskId: string) { if (swipeMap[taskId]) swipeMap[taskId].offset = 0 }

// ── Actions rapides ──────────────────────────────────────────────────────────
function scheduleTonight(taskId: string) {
  const d = new Date(); d.setHours(19, 0, 0, 0)
  tasksStore.updateTask(taskId, { startAt: d.toISOString() })
  closeSwipe(taskId)
}
function scheduleTomorrowMorning(taskId: string) {
  const d = new Date(); d.setDate(d.getDate() + 1); d.setHours(8, 0, 0, 0)
  tasksStore.updateTask(taskId, { startAt: d.toISOString() })
  closeSwipe(taskId)
}

function formatDateTime(dateString: string) {
  const date = new Date(dateString)
  const today = new Date()
  const tomorrow = new Date(today); tomorrow.setDate(today.getDate() + 1)
  const time = date.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
  if (date.toDateString() === today.toDateString()) return `Aujourd'hui à ${time}`
  if (date.toDateString() === tomorrow.toDateString()) return `Demain à ${time}`
  return date.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' }) + ` à ${time}`
}

const isModalOpen = ref(false)
const newTaskTitle = ref('')
const newTaskDuration = ref(1)
const newTaskFinishAt = ref('')
const newTaskTags = ref<string[]>([])
const isCreating = ref(false)

// Tous les tags déjà utilisés dans les tâches existantes (dédupliqués, triés)
const allExistingTags = computed(() => {
  const set = new Set<string>()
  for (const task of tasks.value) {
    for (const tag of task.tags) set.add(tag)
  }
  return [...set].sort()
})

const tableHeaders = [
  { title: 'Tâche', key: 'title', width: '40%' },
  { title: 'Tags', key: 'tags', width: '25%', sortable: false },
  { title: 'Créé le', key: 'createdAt', width: '15%' },
  { title: 'Actions', key: 'actions', width: '10%', sortable: false }
]

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('fr-FR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const openModal = () => {
  newTaskTitle.value = ''
  newTaskDuration.value = 1
  newTaskFinishAt.value = ''
  newTaskTags.value = []
  isModalOpen.value = true
}

const closeModal = () => {
  isModalOpen.value = false
  newTaskTitle.value = ''
  newTaskDuration.value = 1
  newTaskFinishAt.value = ''
  newTaskTags.value = []
}

const createTask = async () => {
  if (!newTaskTitle.value.trim()) return

  isCreating.value = true
  try {
    await tasksStore.addTask(
      newTaskTitle.value,
      newTaskDuration.value,
      null,
      newTaskFinishAt.value || null,
      newTaskTags.value
    )
    closeModal()
  } catch (error) {
    console.error('Erreur lors de la création:', error)
  } finally {
    isCreating.value = false
  }
}

const toggleFinish = async (id: string) => {
  await tasksStore.toggleFinish(id)
}

const removeTask = async (id: string) => {
  if (confirm('Êtes-vous sûr de vouloir supprimer cette tâche ?')) {
    await tasksStore.removeTask(id)
  }
}

const retryLoad = () => {
  tasksStore.fetchTasks()
}

const clearError = () => {
  tasksStore.clearError()
}

onMounted(() => {
  tasksStore.fetchTasks()
  recurringStore.fetchTemplates()
})
</script>
