import { View, Text, StyleSheet, TouchableOpacity, Alert } from 'react-native'
import { useAuth0 } from 'react-native-auth0'
import { Ionicons } from '@expo/vector-icons'
import { tokenStorage } from '@/services/tokenStorage'

export default function SettingsScreen() {
  const { user, clearSession } = useAuth0()

  const handleLogout = () =>
    Alert.alert('Se déconnecter ?', '', [
      { text: 'Annuler', style: 'cancel' },
      {
        text: 'Se déconnecter', style: 'destructive',
        onPress: async () => {
          await tokenStorage.remove()
          await clearSession()
        },
      },
    ])

  return (
    <View style={styles.container}>
      {/* Avatar section */}
      <View style={styles.profileSection}>
        <View style={styles.avatar}>
          <Text style={styles.avatarText}>
            {user?.name?.charAt(0).toUpperCase() ?? '?'}
          </Text>
        </View>
        <Text style={styles.name}>{user?.name}</Text>
        <Text style={styles.email}>{user?.email}</Text>
      </View>

      {/* Info rows */}
      <View style={styles.card}>
        <Row icon="person-outline" label="Identifiant" value={user?.sub ?? '—'} />
        <View style={styles.divider} />
        <Row icon="mail-outline" label="Email" value={user?.email ?? '—'} />
      </View>

      {/* Logout */}
      <TouchableOpacity style={styles.logoutBtn} onPress={handleLogout} activeOpacity={0.8}>
        <Ionicons name="log-out-outline" size={20} color="#e53935" />
        <Text style={styles.logoutText}>Se déconnecter</Text>
      </TouchableOpacity>
    </View>
  )
}

function Row({ icon, label, value }: { icon: any; label: string; value: string }) {
  return (
    <View style={styles.row}>
      <Ionicons name={icon} size={18} color="#666" style={{ width: 24 }} />
      <Text style={styles.rowLabel}>{label}</Text>
      <Text style={styles.rowValue} numberOfLines={1}>{value}</Text>
    </View>
  )
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5', padding: 20 },
  profileSection: { alignItems: 'center', paddingVertical: 24, gap: 6 },
  avatar: { width: 72, height: 72, borderRadius: 36, backgroundColor: '#6200ea', alignItems: 'center', justifyContent: 'center', marginBottom: 8 },
  avatarText: { color: '#fff', fontSize: 30, fontWeight: '700' },
  name: { fontSize: 20, fontWeight: '700', color: '#1a1a1a' },
  email: { fontSize: 14, color: '#666' },
  card: { backgroundColor: '#fff', borderRadius: 14, padding: 4, marginBottom: 24 },
  divider: { height: StyleSheet.hairlineWidth, backgroundColor: '#e0e0e0', marginLeft: 46 },
  row: { flexDirection: 'row', alignItems: 'center', padding: 14, gap: 12 },
  rowLabel: { flex: 1, fontSize: 15, color: '#1a1a1a' },
  rowValue: { flex: 2, fontSize: 14, color: '#666', textAlign: 'right' },
  logoutBtn: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: 8, padding: 16, backgroundColor: '#fff', borderRadius: 14 },
  logoutText: { color: '#e53935', fontSize: 16, fontWeight: '600' },
})
