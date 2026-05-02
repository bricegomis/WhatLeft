import { useState } from 'react'
import {
  Modal, View, Text, TextInput, TouchableOpacity,
  StyleSheet, KeyboardAvoidingView, Platform, ScrollView,
} from 'react-native'
import { useTasksStore } from '@/stores/tasksStore'
import TagChip from './TagChip'

interface Props {
  visible: boolean
  onClose: () => void
}

export default function AddTaskModal({ visible, onClose }: Props) {
  const { addTask, allTags } = useTasksStore()

  const [title, setTitle] = useState('')
  const [duration, setDuration] = useState('30')
  const [tagInput, setTagInput] = useState('')
  const [tags, setTags] = useState<string[]>([])
  const [saving, setSaving] = useState(false)

  const reset = () => { setTitle(''); setDuration('30'); setTagInput(''); setTags([]) }

  const close = () => { reset(); onClose() }

  const addTag = (tag: string) => {
    const t = tag.trim()
    if (t && !tags.includes(t)) setTags(prev => [...prev, t])
    setTagInput('')
  }

  const removeTag = (tag: string) => setTags(prev => prev.filter(t => t !== tag))

  const save = async () => {
    if (!title.trim()) return
    setSaving(true)
    await addTask({ title: title.trim(), duration: Number(duration) || 30, tags })
    setSaving(false)
    close()
  }

  return (
    <Modal visible={visible} animationType="slide" presentationStyle="formSheet" onRequestClose={close}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={{ flex: 1 }}>
        <View style={styles.container}>
          {/* Header */}
          <View style={styles.header}>
            <TouchableOpacity onPress={close}><Text style={styles.cancel}>Annuler</Text></TouchableOpacity>
            <Text style={styles.headerTitle}>Nouvelle tâche</Text>
            <TouchableOpacity onPress={save} disabled={!title.trim() || saving}>
              <Text style={[styles.save, (!title.trim() || saving) && { opacity: 0.4 }]}>Créer</Text>
            </TouchableOpacity>
          </View>

          <ScrollView contentContainerStyle={styles.body} keyboardShouldPersistTaps="handled">
            <Text style={styles.label}>Titre *</Text>
            <TextInput
              style={styles.input}
              placeholder="Ex : Running, rapport…"
              value={title}
              onChangeText={setTitle}
              autoFocus
            />

            <Text style={styles.label}>Durée (minutes)</Text>
            <TextInput
              style={styles.input}
              value={duration}
              onChangeText={setDuration}
              keyboardType="numeric"
            />

            <Text style={styles.label}>Tags</Text>
            <View style={styles.tagInputRow}>
              <TextInput
                style={[styles.input, { flex: 1 }]}
                placeholder="Ajouter un tag…"
                value={tagInput}
                onChangeText={setTagInput}
                onSubmitEditing={() => addTag(tagInput)}
                returnKeyType="done"
              />
              {tagInput.trim().length > 0 && (
                <TouchableOpacity style={styles.addTagBtn} onPress={() => addTag(tagInput)}>
                  <Text style={styles.addTagText}>+</Text>
                </TouchableOpacity>
              )}
            </View>

            {/* Selected tags */}
            {tags.length > 0 && (
              <View style={styles.tagRow}>
                {tags.map(tag => (
                  <TouchableOpacity key={tag} onPress={() => removeTag(tag)}>
                    <TagChip label={`${tag} ✕`} selected />
                  </TouchableOpacity>
                ))}
              </View>
            )}

            {/* Suggestions from existing tags */}
            {allTags.filter(t => !tags.includes(t)).length > 0 && (
              <>
                <Text style={[styles.label, { marginTop: 8 }]}>Suggestions</Text>
                <View style={styles.tagRow}>
                  {allTags.filter(t => !tags.includes(t)).map(tag => (
                    <TagChip key={tag} label={tag} onPress={() => addTag(tag)} />
                  ))}
                </View>
              </>
            )}
          </ScrollView>
        </View>
      </KeyboardAvoidingView>
    </Modal>
  )
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#fff' },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 16, borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: '#e0e0e0' },
  headerTitle: { fontSize: 17, fontWeight: '600' },
  cancel: { fontSize: 16, color: '#666' },
  save: { fontSize: 16, color: '#6200ea', fontWeight: '600' },
  body: { padding: 20, gap: 6, paddingBottom: 40 },
  label: { fontSize: 13, color: '#666', marginBottom: 2 },
  input: { borderWidth: 1, borderColor: '#ddd', borderRadius: 10, padding: 12, fontSize: 15 },
  tagInputRow: { flexDirection: 'row', gap: 8, alignItems: 'center' },
  addTagBtn: { backgroundColor: '#6200ea', borderRadius: 10, paddingHorizontal: 16, paddingVertical: 12 },
  addTagText: { color: '#fff', fontSize: 20, lineHeight: 20 },
  tagRow: { flexDirection: 'row', flexWrap: 'wrap', gap: 6, marginTop: 4 },
})
