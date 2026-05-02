import { useEffect } from 'react'
import { View, Text, FlatList, StyleSheet, TouchableOpacity, RefreshControl } from 'react-native'
import { Ionicons } from '@expo/vector-icons'
import { useTasksStore } from '@/stores/tasksStore'

export default function HistoryScreen() {
  const { history, isLoading, fetchHistory, reactivateTask } = useTasksStore()

  useEffect(() => { fetchHistory() }, [])

  return (
    <FlatList
      data={history}
      keyExtractor={t => t.id}
      refreshControl={<RefreshControl refreshing={isLoading} onRefresh={fetchHistory} />}
      contentContainerStyle={history.length === 0 ? styles.emptyContainer : { paddingBottom: 24 }}
      ItemSeparatorComponent={() => <View style={styles.separator} />}
      ListEmptyComponent={
        <View style={styles.empty}>
          <Ionicons name="time-outline" size={64} color="#ccc" />
          <Text style={styles.emptyText}>Aucun historique</Text>
        </View>
      }
      renderItem={({ item }) => (
        <View style={styles.row}>
          <View style={{ flex: 1 }}>
            <Text style={[styles.title, !!item.finishAt && styles.finished]}>{item.title}</Text>
            <View style={styles.meta}>
              {item.finishAt
                ? <View style={styles.badge}><Ionicons name="checkmark-circle" size={12} color="#2e7d32" /><Text style={[styles.badgeText, { color: '#2e7d32' }]}>Terminée</Text></View>
                : <View style={styles.badge}><Ionicons name="close-circle" size={12} color="#f57c00" /><Text style={[styles.badgeText, { color: '#f57c00' }]}>Annulée</Text></View>
              }
              <Text style={styles.duration}>{item.duration} min</Text>
            </View>
          </View>
          <TouchableOpacity onPress={() => reactivateTask(item.id)} style={styles.reactivateBtn}>
            <Ionicons name="arrow-undo" size={20} color="#6200ea" />
          </TouchableOpacity>
        </View>
      )}
    />
  )
}

const styles = StyleSheet.create({
  emptyContainer: { flex: 1 },
  empty: { alignItems: 'center', justifyContent: 'center', flex: 1, paddingTop: 80, gap: 12 },
  emptyText: { color: '#999', fontSize: 16 },
  separator: { height: StyleSheet.hairlineWidth, backgroundColor: '#e0e0e0', marginLeft: 16 },
  row: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#fff', padding: 16 },
  title: { fontSize: 15, color: '#1a1a1a', marginBottom: 4 },
  finished: { textDecorationLine: 'line-through', color: '#999' },
  meta: { flexDirection: 'row', alignItems: 'center', gap: 8 },
  badge: { flexDirection: 'row', alignItems: 'center', gap: 3 },
  badgeText: { fontSize: 12 },
  duration: { fontSize: 12, color: '#999' },
  reactivateBtn: { padding: 8 },
})
