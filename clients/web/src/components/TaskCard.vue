<template>
  <div
    style="position:relative; overflow:hidden;"
    :class="{ 'fc-event': draggable }"
    :data-event-id="draggable ? task.id : undefined"
    :data-duration="draggable ? task.duration : undefined"
  >
    <!-- Actions révélées au swipe gauche -->
    <div
      v-if="swipeable"
      style="position:absolute; right:0; top:0; bottom:0; display:flex; width:210px;"
    >
      <button
        style="flex:1; background:#34c759; color:white; border:none; cursor:pointer; display:flex; flex-direction:column; align-items:center; justify-content:center; gap:2px;"
        @click="emit('toggle-finish'); closeSwipe()"
      >
        <v-icon size="20">mdi-check</v-icon>
        <span style="font-size:10px;">Terminer</span>
      </button>
      <button
        style="flex:1; background:#ff9500; color:white; border:none; cursor:pointer; display:flex; flex-direction:column; align-items:center; justify-content:center; gap:2px;"
        @click="emit('schedule-tonight'); closeSwipe()"
      >
        <v-icon size="20">mdi-weather-sunset</v-icon>
        <span style="font-size:10px;">Ce soir</span>
      </button>
      <button
        style="flex:1; background:#007aff; color:white; border:none; cursor:pointer; display:flex; flex-direction:column; align-items:center; justify-content:center; gap:2px;"
        @click="emit('schedule-tomorrow'); closeSwipe()"
      >
        <v-icon size="20">mdi-weather-sunrise</v-icon>
        <span style="font-size:10px;">Demain</span>
      </button>
    </div>

    <!-- Contenu principal -->
    <div
      class="d-flex align-center px-4 py-3"
      :style="{
        background: 'white',
        position: 'relative',
        zIndex: 1,
        transform: swipeable ? `translateX(${swipeOffset}px)` : undefined,
        transition: swipeable && isDragging ? 'none' : 'transform 0.25s ease',
        cursor: draggable ? 'grab' : 'pointer',
        userSelect: 'none',
      }"
      @touchstart="swipeable ? onTouchStart($event) : undefined"
      @touchmove="swipeable ? onTouchMove($event) : undefined"
      @touchend="swipeable ? onTouchEnd($event) : undefined"
      @click="swipeable && swipeOffset !== 0 ? closeSwipe() : emit('click')"
    >
      <!-- Checkbox -->
      <v-btn
        :icon="task.finishAt ? 'mdi-check-circle' : 'mdi-circle-outline'"
        :color="task.finishAt ? 'success' : 'grey-lighten-1'"
        variant="plain"
        size="small"
        :disabled="disabled"
        class="mr-3 flex-shrink-0"
        @click.stop="emit('toggle-finish')"
      />

      <!-- Texte -->
      <div class="flex-grow-1" style="min-width:0; position:relative;">
        <span style="position:absolute; top:0; right:0; font-size:10px; color:#999; white-space:nowrap;">
          {{ task.duration }} min
        </span>
        <div :class="['text-body-1', { 'text-decoration-line-through text-medium-emphasis': task.finishAt }]">
          {{ task.title }}
        </div>
        <div v-if="task.startAt || task.tags.length" class="d-flex align-center flex-wrap gap-1 mt-1">
          <span v-if="task.startAt" class="text-caption text-primary d-flex align-center ga-1">
            <v-icon size="12">mdi-clock-outline</v-icon>{{ formattedDate }}
          </span>
          <v-chip v-for="tag in task.tags" :key="tag" size="x-small" variant="tonal" color="primary">
            {{ tag }}
          </v-chip>
        </div>
        <div v-if="recurrenceType" style="position:absolute; bottom:0; right:0;">
          <v-tooltip
            :text="recurrenceType === 'Daily' ? 'Journalier' : recurrenceType === 'Monthly' ? 'Mensuel' : 'Hebdomadaire'"
            location="start"
          >
            <template #activator="{ props }">
              <v-icon v-bind="props" size="14" color="deep-purple-lighten-2">
                {{ recurrenceType === 'Daily' ? 'mdi-calendar-today' : recurrenceType === 'Monthly' ? 'mdi-calendar-month' : 'mdi-calendar-week' }}
              </v-icon>
            </template>
          </v-tooltip>
        </div>
      </div>

      <!-- Bouton supprimer (desktop) -->
      <v-btn
        class="d-none d-sm-flex flex-shrink-0 ml-2"
        icon="mdi-delete-outline"
        variant="plain"
        color="grey"
        size="small"
        :disabled="disabled"
        @click.stop="emit('delete')"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { Task } from '../stores/tasks'

const props = defineProps<{
  task: Task
  recurrenceType?: 'Daily' | 'Weekly' | 'Monthly' | null
  swipeable?: boolean
  draggable?: boolean
  disabled?: boolean
}>()

const emit = defineEmits<{
  'click': []
  'toggle-finish': []
  'schedule-tonight': []
  'schedule-tomorrow': []
  'delete': []
}>()

// ── Date formatting ───────────────────────────────────────────────────────────
const formattedDate = computed(() => {
  if (!props.task.startAt) return ''
  const date = new Date(props.task.startAt)
  const today = new Date()
  const tomorrow = new Date(today)
  tomorrow.setDate(today.getDate() + 1)
  const time = date.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
  if (date.toDateString() === today.toDateString()) return `Aujourd'hui à ${time}`
  if (date.toDateString() === tomorrow.toDateString()) return `Demain à ${time}`
  return date.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' }) + ` à ${time}`
})

// ── Swipe state (interne) ─────────────────────────────────────────────────────
const ACTION_WIDTH = 210
const swipeOffset = ref(0)
const isDragging = ref(false)
const startX = ref(0)
const startY = ref(0)
const baseOffset = ref(0)
const lockAxis = ref<'h' | 'v' | null>(null)

function closeSwipe() { swipeOffset.value = 0 }

function onTouchStart(e: TouchEvent) {
  startX.value = e.touches[0].clientX
  startY.value = e.touches[0].clientY
  baseOffset.value = swipeOffset.value
  isDragging.value = true
  lockAxis.value = null
}

function onTouchMove(e: TouchEvent) {
  if (!isDragging.value) return
  const dx = e.touches[0].clientX - startX.value
  const dy = e.touches[0].clientY - startY.value
  if (lockAxis.value === null) {
    if (Math.abs(dx) > 8 || Math.abs(dy) > 8)
      lockAxis.value = Math.abs(dx) >= Math.abs(dy) ? 'h' : 'v'
    return
  }
  if (lockAxis.value === 'h') {
    e.preventDefault()
    swipeOffset.value = Math.max(-ACTION_WIDTH, Math.min(0, baseOffset.value + dx))
  }
}

function onTouchEnd(e: TouchEvent) {
  if (lockAxis.value === 'h') {
    const dx = e.changedTouches[0].clientX - startX.value
    if (dx < -60) swipeOffset.value = -ACTION_WIDTH
    else if (dx > 30) swipeOffset.value = 0
    else swipeOffset.value = baseOffset.value
  }
  isDragging.value = false
  lockAxis.value = null
}
</script>

