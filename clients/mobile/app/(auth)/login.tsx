import { View, Text, StyleSheet, TouchableOpacity, ActivityIndicator } from 'react-native'
import { useAuth0 } from 'react-native-auth0'
import { Ionicons } from '@expo/vector-icons'

export default function LoginScreen() {
  const { authorize, error, isLoading } = useAuth0()

  async function handleLogin() {
    await authorize({ audience: 'https://whatleft-api', scope: 'openid profile email offline_access' })
  }

  return (
    <View style={styles.container}>
      <View style={styles.hero}>
        <Ionicons name="checkmark-done-circle" size={80} color="#6200ea" />
        <Text style={styles.title}>WhatLeft</Text>
        <Text style={styles.subtitle}>
          Gérez vos tâches, disponible partout.
        </Text>
      </View>

      {error && (
        <Text style={styles.errorText}>{error.message}</Text>
      )}

      <TouchableOpacity
        style={[styles.btn, isLoading && styles.btnDisabled]}
        onPress={handleLogin}
        disabled={isLoading}
        activeOpacity={0.8}
      >
        {isLoading
          ? <ActivityIndicator color="#fff" />
          : <Text style={styles.btnText}>Se connecter</Text>
        }
      </TouchableOpacity>
    </View>
  )
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: 80,
    paddingHorizontal: 32,
  },
  hero: { alignItems: 'center', gap: 16 },
  title: { fontSize: 36, fontWeight: '700', color: '#1a1a1a' },
  subtitle: { fontSize: 16, color: '#666', textAlign: 'center', lineHeight: 24 },
  errorText: { color: '#e53935', fontSize: 14, textAlign: 'center' },
  btn: {
    width: '100%',
    backgroundColor: '#6200ea',
    paddingVertical: 16,
    borderRadius: 14,
    alignItems: 'center',
  },
  btnDisabled: { opacity: 0.6 },
  btnText: { color: '#fff', fontSize: 16, fontWeight: '600' },
})
