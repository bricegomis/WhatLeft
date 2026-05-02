import { useState, useEffect, useCallback } from 'react'
import {
  View, Text, FlatList, StyleSheet, TouchableOpacity,
  RefreshControl, TextInput, Modal, ScrollView,
  KeyboardAvoidingView, Platform, Alert,
} from 'react-native'
import { Ionicons } from '@expo/vector-icons'
import { GestureDetector, Gesture } from 'react-native-gesture-handler'
import Animated, { useSharedValue, useAnimatedStyle, withSpring, runOnJS } from 'react-native-reanimated'
import { useTasksStore } from '@/stores/tasksStore'
import { useRecurringStore } from '@/stores/recurringStore'
import type { Task } from '@/types/task'
import TagChip from '@/components/TagChip'
import AddTaskModal from '@/components/AddTaskModal'

export default function TasksScreen() {
  const {
    tasks, isLoading, error,
    fetchTasks, toggleFinish, removeTask, updateTask, clearError,
    allTags,
  } = useTasksStore()
  const { templates, fetchTemplates } = useRecurringStore()

  const [filterTags, setFilterTags] = useState<string[]>([])
  const [showAdd, setShowAdd] = useState(false)
  const [editingTask, setEditingTask] = useState<Task | null>(null)

  useEffect(() => { fetchTasks(); fetchTemplates() }, [])

  const pending = tasks.filter(t => !t.finishAt && !t.cancelledAt)
  const filtered = filterTags.length === 0
    ? pending
    : pending.filter(t => filterTags.every(tag => t.tags.includes(tag)))

  const toggleTag = (tag: string) =>
    setFilterTags(prev => prev.includes(tag) ? prev.filter(t => t !== tag) : [...prev, tag])

  function getRecurrenceIcon(task: Task) {
    if (!task.recurringTaskTemplateId) return null
    const tpl = templates.find(t => t.id === task.recurringTaskTemplateId)
    if (!tpl) return null
    return tpl.recurrenceType === 'Daily' ? 'calendar' :
           tpl.recurrenceType === 'Monthly' ? 'calendar-number' : 'calendar-outline'
  }

  const handleDelete = (id: string) =>
    Alert.alert('Supprimer ?', 'Cette action est irréversible.', [
      { text: 'Annuler', style: 'cancel' },
      { text: 'Supprimer', style: 'destructive', onPress: () => removeTask(id) },
    ])

  return (
    <View style={styles.container}>
      {/* Tag filters */}
      {allTags.length > 0 && (
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.tagBar}
        >
          {allTags.map(tag => (
            <TagChip
              key={tag}
              label={tag}
              selected={filterTags.includes(tag)}
              onPress={() => toggleTag(tag)}
            />
          ))}
          {filterTags.length > 0 && (
            <TouchableOpacity onPress={() => setFilterTags([])} style={styles.resetChip}>
              <Text style={styles.resetText}>✕ Réinit.</Text>
            </TouchableOpacity>
          )}
        </ScrollView>
      )}

      {error && (
        <View style={styles.errorBar}>
          <Text style={styles.errorText}>{error}</Text>
          <TouchableOpacity onPress={clearError}>
            <Ionicons name="close" size={18} color="#fff" />
          </TouchableOpacity>
        </View>
      )}

      <FlatList
        data={filtered}
        keyExtractor={t => t.id}
        refreshControl={<RefreshControl refreshing={isLoading} onRefresh={fetchTasks} />}
        contentContainerStyle={filtered.length === 0 ? styles.emptyContainer : { paddingBottom: 100 }}
        ListEmptyComponent={
          <View style={styles.empty}>
            <Ionicons name="checkmark-done-circle-outline" size={64} color="#ccc" />
            <Text style={styles.emptyText}>
              {filterTags.length > 0 ? 'Aucune tâche pour ces tags' : 'Aucune tâche en cours'}
            </Text>
          </View>
        }
        ItemSeparatorComponent={() => <View style={styles.separator} />}
        renderItem={({ item }) => (
          <SwipeableTaskRow
            task={item}
            recurrenceIcon={getRecurrenceIcon(item)}
            onToggle={() => toggleFinish(item.id)}
            onEdit={() => setEditingTask(item)}
            onDelete={() => handleDelete(item.id)}
          />
        )}
      />

      {/* FAB */}
      <TouchableOpacity style={styles.fab} onPress={() => setShowAdd(true)} activeOpacity={0.85}>
        <Ionicons name="add" size={32} color="#fff" />
      </TouchableOpacity>

      <AddTaskModal visible={showAdd} onClose={() => setShowAdd(false)} />
      {editingTask && (
        <EditTaskModal
          task={editingTask}
          onClose={() => setEditingTask(null)}
          onSave={async (req) => { await updateTask(editingTask.id, req); setEditingTask(null) }}
          onDelete={() => { handleDelete(editingTask.id); setEditingTask(null) }}
          onFinish={() => { toggleFinish(editingTask.id); setEditingTask(null) }}
        />
      )}
    </View>
  )
}

// ── Swipeable row ──────────────────────────────────────────────────────────

function SwipeableTaskRow({
  task, recurrenceIcon, onToggle, onEdit, onDelete
}: {
  task: Task
  recurrenceIcon: string | null
  onToggle: () => void
  onEdit: () => void
  onDelete: () => void
}) {
  const ACTION_WIDTH = 180
  const translateX = useSharedValue(0)

  const panGesture = Gesture.Pan()
    .activeOffsetX([-10, 10])
    .failOffsetY([-5, 5])
    .onUpdate(e => {
      translateX.value = Math.max(-ACTION_WIDTH, Math.min(0, e.translationX))
    })
    .onEnd(e => {
      if (e.translationX < -60) translateX.value = withSpring(-ACTION_WIDTH)
      else translateX.value = withSpring(0)
    })

  const rowStyle = useAnimatedStyle(() => ({
    transform: [{ translateX: translateX.value }],
  }))

  const formatDate = (d: string | null) => {
    if (!d) return null
    const date = new Date(d)
    const today = new Date()
    const tomorrow = new Date(today); tomorrow.setDate(today.getDate() + 1)
    const time = date.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
    if (date.toDateString() === today.toDateString()) return `Aujourd'hui ${time}`
    if (date.toDateString() === tomorrow.toDateString()) return `Demain ${time}`
    return date.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short' }) + ` ${time}`
  }

  return (
    <View style={{ overflow: 'hidden' }}>
      {/* Action buttons behind */}
      <View style={[styles.actions, { width: ACTION_WIDTH }]}>
        <TouchableOpacity style={[styles.actionBtn, { backgroundColor: '#34c759' }]} onPress={onToggle}>
          <Ionicons name="checkmark" size={22} color="#fff" />
          <Text style={styles.actionLabel}>Terminer</Text>
        </TouchableOpacity>
        <TouchableOpacity style={[styles.actionBtn, { backgroundColor: '#ff3b30' }]} onPress={onDelete}>
          <Ionicons name="trash" size={22} color="#fff" />
          <Text style={styles.actionLabel}>Suppr.</Text>
        </TouchableOpacity>
      </View>

      <GestureDetector gesture={panGesture}>
        <Animated.View style={[styles.row, rowStyle]}>
          {/* Checkbox */}
          <TouchableOpacity onPress={onToggle} style={styles.checkBtn}>
            <Ionicons
              name={task.finishAt ? 'checkmark-circle' : 'ellipse-outline'}
              size={26}
              color={task.finishAt ? '#34c759' : '#ccc'}
            />
          </TouchableOpacity>

          {/* Content */}
          <TouchableOpacity style={styles.rowContent} onPress={onEdit} activeOpacity={0.7}>
            <View style={styles.rowTop}>
              <Text style={[styles.taskTitle, task.finishAt && styles.done]} numberOfLines={2}>
                {task.title}
              </Text>
              <Text style={styles.duration}>{task.duration} min</Text>
            </View>
            <View style={styles.rowMeta}>
              {task.startAt && (
                <View style={styles.dateTag}>
                  <Ionicons name="time-outline" size={11} color="#6200ea" />
                  <Text style={styles.dateText}>{formatDate(task.startAt)}</Text>
                </View>
              )}
              {task.tags.map(tag => <TagChip key={tag} label={tag} size="xs" />)}
              {recurrenceIcon && (
                <Ionicons name={recurrenceIcon as any} size={13} color="#9c27b0" />
              )}
            </View>
          </TouchableOpacity>
        </Animated.View>
      </GestureDetector>
    </View>
  )
}

// ── Edit modal ─────────────────────────────────────────────────────────────

function EditTaskModal({ task, onClose, onSave, onDelete, onFinish }: {
  task: Task
  onClose: () => void
  onSave: (req: any) => Promise<void>
  onDelete: () => void
  onFinish: () => void
}) {
  const [title, setTitle] = useState(task.title)
  const [duration, setDuration] = useState(String(task.duration))
  const [saving, setSaving] = useState(false)

  const save = async () => {
    setSaving(true)
    await onSave({ title, duration: Number(duration) })
    setSaving(false)
  }

  return (
    <Modal visible animationType="slide" presentationStyle="formSheet" onRequestClose={onClose}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={{ flex: 1 }}>
        <View style={styles.modal}>
          <View style={styles.modalHeader}>
            <TouchableOpacity onPress={onClose}><Text style={styles.modalCancel}>Annuler</Text></TouchableOpacity>
            <Text style={styles.modalTitle}>Modifier</Text>
            <TouchableOpacity onPress={save} disabled={saving}>
              <Text style={[styles.modalSave, saving && { opacity: 0.4 }]}>Enregistrer</Text>
            </TouchableOpacity>
          </View>
          <View style={styles.modalBody}>
            <Text style={styles.label}>Titre</Text>
            <TextInput style={styles.input} value={title} onChangeText={setTitle} />
            <Text style={styles.label}>Durée (min)</Text>
            <TextInput style={styles.input} value={duration} onChangeText={setDuration} keyboardType="numeric" />
            <View style={styles.modalActions}>
              <TouchableOpacity style={styles.deleteBtn} onPress={onDelete}>
                <Text style={styles.deleteBtnText}>Supprimer</Text>
              </TouchableOpacity>
              {!task.finishAt && (
                <TouchableOpacity style={styles.finishBtn} onPress={onFinish}>
                  <Text style={styles.finishBtnText}>Terminer</Text>
                </TouchableOpacity>
              )}
            </View>
          </View>
        </View>
      </KeyboardAvoidingView>
    </Modal>
  )
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5' },
  tagBar: { paddingHorizontal: 12, paddingVertical: 8, gap: 6 },
  resetChip: { paddingHorizontal: 12, paddingVertical: 5, backgroundColor: '#eee', borderRadius: 20 },
  resetText: { fontSize: 12, color: '#666' },
  errorBar: { flexDirection: 'row', backgroundColor: '#e53935', padding: 10, justifyContent: 'space-between', alignItems: 'center', paddingHorizontal: 16 },
  errorText: { color: '#fff', flex: 1, fontSize: 13 },
  emptyContainer: { flex: 1 },
  empty: { alignItems: 'center', justifyContent: 'center', flex: 1, paddingTop: 80, gap: 12 },
  emptyText: { color: '#999', fontSize: 16 },
  separator: { height: StyleSheet.hairlineWidth, backgroundColor: '#e0e0e0', marginLeft: 58 },
  // Row
  row: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#fff', paddingVertical: 12, paddingRight: 12 },
  checkBtn: { paddingHorizontal: 14 },
  rowContent: { flex: 1 },
  rowTop: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start' },
  taskTitle: { fontSize: 15, color: '#1a1a1a', flex: 1, marginRight: 8 },
  done: { textDecorationLine: 'line-through', color: '#999' },
  duration: { fontSize: 11, color: '#999' },
  rowMeta: { flexDirection: 'row', flexWrap: 'wrap', alignItems: 'center', gap: 4, marginTop: 4 },
  dateTag: { flexDirection: 'row', alignItems: 'center', gap: 3 },
  dateText: { fontSize: 11, color: '#6200ea' },
  // Actions
  actions: { position: 'absolute', right: 0, top: 0, bottom: 0, flexDirection: 'row' },
  actionBtn: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 3 },
  actionLabel: { color: '#fff', fontSize: 10, fontWeight: '600' },
  // FAB
  fab: { position: 'absolute', bottom: 28, right: 24, width: 58, height: 58, borderRadius: 29, backgroundColor: '#6200ea', alignItems: 'center', justifyContent: 'center', elevation: 6, shadowColor: '#000', shadowOffset: { width: 0, height: 3 }, shadowOpacity: 0.3, shadowRadius: 6 },
  // Modal
  modal: { flex: 1, backgroundColor: '#fff' },
  modalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 16, borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: '#e0e0e0' },
  modalTitle: { fontSize: 17, fontWeight: '600' },
  modalCancel: { fontSize: 16, color: '#666' },
  modalSave: { fontSize: 16, color: '#6200ea', fontWeight: '600' },
  modalBody: { padding: 20, gap: 8 },
  label: { fontSize: 13, color: '#666', marginBottom: 4 },
  input: { borderWidth: 1, borderColor: '#ddd', borderRadius: 10, padding: 12, fontSize: 15 },
  modalActions: { flexDirection: 'row', gap: 12, marginTop: 16 },
  deleteBtn: { flex: 1, padding: 14, borderRadius: 10, backgroundColor: '#ffeaea', alignItems: 'center' },
  deleteBtnText: { color: '#e53935', fontWeight: '600' },
  finishBtn: { flex: 1, padding: 14, borderRadius: 10, backgroundColor: '#e8f5e9', alignItems: 'center' },
  finishBtnText: { color: '#2e7d32', fontWeight: '600' },
})
