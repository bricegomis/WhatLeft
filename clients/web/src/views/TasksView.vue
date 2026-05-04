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

    <!-- Filtres tags -->
    <v-row v-if="tasks.length > 0" class="mb-2" dense align="center">
      <v-col cols="12" sm>
        <v-autocomplete
          v-model="filterTags"
          :items="allExistingTags"
          label="Filtrer par tags"
          multiple
          chips
          closable-chips
          clearable
          density="compact"
          variant="outlined"
          hide-details
          prepend-inner-icon="mdi-tag-outline"
        />
      </v-col>
      <v-col v-if="filterTags.length > 0" cols="auto">
        <v-btn variant="text" size="small" color="primary" @click="filterTags = []">Réinitialiser</v-btn>
      </v-col>
    </v-row>

    <!-- Loading State -->
    <div v-if="isLoading && tasks.length === 0" class="text-center pa-12">
      <v-progress-circular indeterminate color="primary" class="mb-4" />
      <p class="text-body-1">Chargement des tâches...</p>
    </div>

    <!-- Tasks Table -->
    <v-card v-else-if="tasks.length > 0 && viewMode === 'table'" class="mb-6">
      <v-data-table
        :headers="tableHeaders"
        :items="filteredTasks"
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
      <template v-for="(task, index) in filteredTasks" :key="task.id">
        <TaskCard
          :task="task"
          :recurrence-type="taskRecurrenceType(task)"
          :swipeable="true"
          :disabled="isLoading"
          @click="openEditDialog(task)"
          @toggle-finish="toggleFinish(task.id)"
          @schedule-tonight="scheduleTonight(task.id)"
          @schedule-tomorrow="scheduleTomorrowMorning(task.id)"
          @delete="removeTask(task.id)"
        />
        <v-divider v-if="index < filteredTasks.length - 1" />
      </template>
    </v-card>

    <!-- Empty State -->
    <v-card v-else class="text-center pa-12">
      <template v-if="filterTags.length > 0">
        <v-icon color="grey" size="64" class="mb-4">mdi-tag-off-outline</v-icon>
        <h2 class="text-h6 mb-2">Aucune tâche pour ces tags</h2>
        <p class="text-body-2 text-medium-emphasis mb-6">Essayez d'autres tags ou réinitialisez le filtre.</p>
        <v-btn variant="tonal" color="primary" @click="filterTags = []">Réinitialiser</v-btn>
      </template>
      <template v-else>
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
      </template>
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
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useDisplay } from 'vuetify'
import { storeToRefs } from 'pinia'
import { useTasksStore } from '../stores/tasks'
import { useRecurringStore } from '../stores/recurring'
import AdminLayout from '../layouts/AdminLayout.vue'
import TaskCard from '../components/TaskCard.vue'
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

// ── Actions rapides ──────────────────────────────────────────────────────────
function scheduleTonight(taskId: string) {
  const d = new Date(); d.setHours(19, 0, 0, 0)
  tasksStore.updateTask(taskId, { startAt: d.toISOString() })
}
function scheduleTomorrowMorning(taskId: string) {
  const d = new Date(); d.setDate(d.getDate() + 1); d.setHours(8, 0, 0, 0)
  tasksStore.updateTask(taskId, { startAt: d.toISOString() })
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

const filterTags = ref<string[]>([])

const filteredTasks = computed(() => {
  if (filterTags.value.length === 0) return tasks.value
  return tasks.value.filter(t => filterTags.value.every(tag => t.tags.includes(tag)))
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
