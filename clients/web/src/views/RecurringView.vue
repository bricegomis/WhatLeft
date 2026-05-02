<template>
  <AdminLayout>


    <v-alert v-if="hasError" type="error" class="mb-6" dismissible @click:close="clearError">
      {{ error }}
    </v-alert>

    <!-- Boutons reset global -->
    <v-row class="mb-4" dense>
      <v-col cols="12" sm="6">
        <v-btn
          block
          variant="tonal"
          color="deep-purple"
          prepend-icon="mdi-skip-next"
          :disabled="isLoading || !hasDailyActive"
          @click="confirmAdvanceAll('Daily')"
        >
          Terminer la journée
        </v-btn>
      </v-col>
      <v-col cols="12" sm="6">
        <v-btn
          block
          variant="tonal"
          color="indigo"
          prepend-icon="mdi-calendar-arrow-right"
          :disabled="isLoading || !hasWeeklyActive"
          @click="confirmAdvanceAll('Weekly')"
        >
            Terminer la semaine
        </v-btn>
      </v-col>
    </v-row>

    <div v-if="isLoading && templates.length === 0" class="text-center pa-12">
      <v-progress-circular indeterminate color="primary" class="mb-4" />
      <p class="text-body-1">Chargement...</p>
    </div>

    <!-- Liste des templates actifs -->
    <template v-if="activeTemplates.length > 0">
      <h3 class="text-subtitle-1 font-weight-medium mb-3">Actives</h3>
      <v-card class="mb-6">
        <v-list lines="two">
          <template v-for="(tpl, idx) in activeTemplates" :key="tpl.id">
            <v-list-item>
              <template #prepend>
                <v-icon color="primary" class="mr-2">
                  {{ tpl.recurrenceType === 'Daily' ? 'mdi-calendar-today' : tpl.recurrenceType === 'Monthly' ? 'mdi-calendar-month' : 'mdi-calendar-week' }}
                </v-icon>
              </template>

              <v-list-item-title class="font-weight-medium">{{ tpl.title }}</v-list-item-title>
              <v-list-item-subtitle>
                {{ recurrenceLabel(tpl) }}
                <span v-if="tpl.tags.length" class="ms-1">
                  &nbsp;·&nbsp;
                  <v-chip
                    v-for="tag in tpl.tags"
                    :key="tag"
                    size="x-small"
                    variant="tonal"
                    color="primary"
                    class="me-1"
                  >{{ tag }}</v-chip>
                </span>
              </v-list-item-subtitle>

              <template #append>
                <v-btn
                  icon="mdi-play-circle-outline"
                  variant="plain"
                  color="success"
                  size="small"
                  title="Déclencher maintenant"
                  :disabled="isLoading"
                  @click="triggerNow(tpl.id)"
                />
                <v-btn
                  icon="mdi-pencil-outline"
                  variant="plain"
                  color="grey"
                  size="small"
                  @click="openEditDialog(tpl)"
                />
                <v-btn
                  icon="mdi-pause-circle-outline"
                  variant="plain"
                  color="warning"
                  size="small"
                  title="Désactiver"
                  :disabled="isLoading"
                  @click="deactivate(tpl.id)"
                />
              </template>
            </v-list-item>
            <v-divider v-if="idx < activeTemplates.length - 1" inset />
          </template>
        </v-list>
      </v-card>
    </template>

    <!-- Templates inactifs -->
    <template v-if="inactiveTemplates.length > 0">
      <h3 class="text-subtitle-1 font-weight-medium mb-3 text-medium-emphasis">Inactives</h3>
      <v-card class="mb-6" color="grey-lighten-4">
        <v-list lines="two">
          <template v-for="(tpl, idx) in inactiveTemplates" :key="tpl.id">
            <v-list-item>
              <template #prepend>
                <v-icon color="grey" class="mr-2">mdi-repeat-off</v-icon>
              </template>
              <v-list-item-title class="text-medium-emphasis">{{ tpl.title }}</v-list-item-title>
              <v-list-item-subtitle>{{ recurrenceLabel(tpl) }}</v-list-item-subtitle>
              <template #append>
                <v-btn
                  icon="mdi-play-circle-outline"
                  variant="plain"
                  color="success"
                  size="small"
                  title="Réactiver"
                  :disabled="isLoading"
                  @click="activate(tpl.id)"
                />
              </template>
            </v-list-item>
            <v-divider v-if="idx < inactiveTemplates.length - 1" inset />
          </template>
        </v-list>
      </v-card>
    </template>

    <!-- Empty -->
    <v-card v-if="!isLoading && templates.length === 0" class="text-center pa-12">
      <v-icon color="primary" size="64" class="mb-4">mdi-repeat</v-icon>
      <h2 class="text-h6 mb-2">Aucune récurrence</h2>
      <p class="text-body-2 text-medium-emphasis mb-6">
        Créez un modèle de tâche récurrente pour automatiser votre planning.
      </p>
      <v-btn color="primary" prepend-icon="mdi-plus" @click="openCreateDialog">
        Créer une récurrence
      </v-btn>
    </v-card>

    <!-- Dialog confirmation avancement global -->
    <v-dialog v-model="advanceAllDialog" max-width="420" persistent>
      <v-card>
        <v-card-title class="d-flex align-center ga-2">
          <v-icon color="deep-purple">{{ advanceAllType === 'Daily' ? 'mdi-skip-next' : 'mdi-calendar-arrow-right' }}</v-icon>
          {{ advanceAllType === 'Daily' ? 'Passer à demain' : 'Passer à la semaine prochaine' }}
        </v-card-title>
        <v-card-text>
          Toutes les tâches non terminées
          <strong>{{ advanceAllType === 'Daily' ? "d'aujourd'hui" : 'de cette semaine' }}</strong>
          seront annulées et remplacées par de nouvelles tâches
          <strong>{{ advanceAllType === 'Daily' ? 'de demain' : 'de la semaine prochaine' }}</strong>.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="advanceAllDialog = false" :disabled="isAdvancingAll">Annuler</v-btn>
          <v-btn color="deep-purple" :loading="isAdvancingAll" @click="doAdvanceAll">Confirmer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Dialog confirmation avancement période -->
    <v-dialog v-model="advanceDialog" max-width="420" persistent>
      <v-card v-if="advancingTemplate">
        <v-card-title class="d-flex align-center ga-2">
          <v-icon :color="advancingTemplate.recurrenceType === 'Daily' ? 'deep-purple' : 'deep-purple'">
            {{ advancingTemplate.recurrenceType === 'Daily' ? 'mdi-skip-next' : 'mdi-calendar-arrow-right' }}
          </v-icon>
          {{ advancingTemplate.recurrenceType === 'Daily' ? 'Passer à demain' : 'Passer à la semaine prochaine' }}
        </v-card-title>
        <v-card-text>
          <p>
            Les tâches non terminées de
            <strong>{{ advancingTemplate.recurrenceType === 'Daily' ? "aujourd'hui" : 'cette semaine' }}</strong>
            pour <strong>« {{ advancingTemplate.title }} »</strong> seront annulées,
            et une nouvelle tâche sera créée pour
            <strong>{{ advancingTemplate.recurrenceType === 'Daily' ? 'demain' : 'la semaine prochaine' }}</strong>.
          </p>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="advanceDialog = false" :disabled="isAdvancing">Annuler</v-btn>
          <v-btn color="deep-purple" :loading="isAdvancing" @click="doAdvance">Confirmer</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Dialog création / édition -->
    <v-dialog v-model="dialog" max-width="520" :fullscreen="$vuetify.display.xs">
      <v-card>
        <v-card-title>{{ editingId ? 'Modifier la récurrence' : 'Nouvelle récurrence' }}</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="form.title"
            label="Titre de la tâche"
            placeholder="Ex : Running, Mobilité…"
            class="mb-3"
            required
          />

          <v-text-field
            v-model.number="form.duration"
            label="Durée (minutes)"
            type="number"
            min="1"
            step="5"
            class="mb-3"
          />

          <v-select
            v-model="form.recurrenceType"
            :items="recurrenceOptions"
            item-title="label"
            item-value="value"
            label="Récurrence"
            class="mb-3"
            :disabled="!!editingId"
          />

          <v-combobox
            v-model="form.tags"
            :items="allTags"
            label="Tags"
            placeholder="Sélectionner ou créer…"
            multiple
            chips
            closable-chips
            clearable
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="closeDialog">Annuler</v-btn>
          <v-btn
            color="primary"
            :disabled="!form.title.trim() || isSaving"
            :loading="isSaving"
            @click="save"
          >
            {{ editingId ? 'Enregistrer' : 'Créer' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- FAB ajout récurrence -->
    <v-btn
      color="primary"
      icon
      size="large"
      style="position: fixed; bottom: 28px; right: 28px; z-index: 1000;"
      :disabled="isLoading"
      @click="openCreateDialog"
      elevation="4"
    >
      <v-icon size="28">mdi-plus</v-icon>
      <v-tooltip activator="parent" location="start">Nouvelle récurrence</v-tooltip>
    </v-btn>

  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import AdminLayout from '../layouts/AdminLayout.vue'
import { useRecurringStore } from '../stores/recurring'
import { useTasksStore } from '../stores/tasks'
import type { RecurringTaskTemplate, RecurrenceType } from '../types/recurring'

const recurringStore = useRecurringStore()
const { templates, isLoading, hasError, error, activeTemplates, inactiveTemplates } = storeToRefs(recurringStore)

const tasksStore = useTasksStore()
const allTags = computed(() => {
  const set = new Set<string>()
  for (const t of tasksStore.tasks) for (const tag of t.tags) set.add(tag)
  for (const t of templates.value) for (const tag of t.tags) set.add(tag)
  return [...set].sort()
})

const dialog = ref(false)
const isSaving = ref(false)
const editingId = ref<string | null>(null)

const defaultForm = () => ({
  title: '',
  duration: 30,
  recurrenceType: 'Weekly' as RecurrenceType,
  tags: [] as string[]
})
const form = ref(defaultForm())

const recurrenceOptions = [
  { label: 'Hebdomadaire', value: 'Weekly' },
  { label: 'Quotidienne', value: 'Daily' },
  { label: 'Mensuelle', value: 'Monthly' }
]

function recurrenceLabel(tpl: RecurringTaskTemplate) {
  const type = tpl.recurrenceType === 'Weekly' ? 'semaine' : tpl.recurrenceType === 'Monthly' ? 'mois' : 'jour'
  return `1 fois par ${type} · ${tpl.duration} min`
}

function openCreateDialog() {
  editingId.value = null
  form.value = defaultForm()
  dialog.value = true
}

function openEditDialog(tpl: RecurringTaskTemplate) {
  editingId.value = tpl.id
  form.value = {
    title: tpl.title,
    duration: tpl.duration,
    recurrenceType: tpl.recurrenceType,
    tags: [...tpl.tags]
  }
  dialog.value = true
}

function closeDialog() {
  dialog.value = false
  editingId.value = null
  form.value = defaultForm()
}

async function save() {
  if (!form.value.title.trim()) return
  isSaving.value = true
  try {
    if (editingId.value) {
      await recurringStore.updateTemplate(editingId.value, {
        title: form.value.title,
        duration: form.value.duration,
        tags: form.value.tags
      })
    } else {
      await recurringStore.addTemplate({
        title: form.value.title,
        duration: form.value.duration,
        recurrenceType: form.value.recurrenceType,
        tags: form.value.tags
      })
    }
    closeDialog()
  } finally {
    isSaving.value = false
  }
}

async function deactivate(id: string) {
  await recurringStore.deactivateTemplate(id)
}

async function activate(id: string) {
  await recurringStore.activateTemplate(id)
}

async function triggerNow(id: string) {
  await recurringStore.processNow(id)
  // Reload tasks so the newly created instances appear
  await tasksStore.fetchTasks()
}

const advanceAllDialog = ref(false)
const advanceAllType = ref<'Daily' | 'Weekly'>('Daily')
const isAdvancingAll = ref(false)

const hasDailyActive  = computed(() => activeTemplates.value.some(t => t.recurrenceType === 'Daily'))
const hasWeeklyActive = computed(() => activeTemplates.value.some(t => t.recurrenceType === 'Weekly'))

function confirmAdvanceAll(type: 'Daily' | 'Weekly') {
  advanceAllType.value = type
  advanceAllDialog.value = true
}

async function doAdvanceAll() {
  isAdvancingAll.value = true
  try {
    await recurringStore.advanceAllByType(advanceAllType.value)
    await tasksStore.fetchTasks()
    advanceAllDialog.value = false
  } finally {
    isAdvancingAll.value = false
  }
}

const advanceDialog = ref(false)
const advancingTemplate = ref<RecurringTaskTemplate | null>(null)
const isAdvancing = ref(false)

async function doAdvance() {
  if (!advancingTemplate.value) return
  isAdvancing.value = true
  try {
    await recurringStore.advance(advancingTemplate.value.id)
    await tasksStore.fetchTasks()
    advanceDialog.value = false
  } finally {
    isAdvancing.value = false
    advancingTemplate.value = null
  }
}

function clearError() {
  recurringStore.clearError()
}

onMounted(() => {
  recurringStore.fetchTemplates()
})
</script>
