import { useEffect } from 'react'
import { Stack, useRouter, useSegments } from 'expo-router'
import { useAuth0, Auth0Provider } from 'react-native-auth0'
import { GestureHandlerRootView } from 'react-native-gesture-handler'
import { tokenStorage } from '@/services/tokenStorage'

const AUTH0_DOMAIN = 'dev-frvj0skig142mzhh.eu.auth0.com'
const AUTH0_CLIENT_ID = 'VudQOhonXNUNVMMEBwwVCOpTta8bZ2bC'

function AuthGate() {
  const { user, getCredentials, isLoading } = useAuth0()
  const segments = useSegments()
  const router = useRouter()

  useEffect(() => {
    if (isLoading) return

    const inAuthGroup = segments[0] === '(auth)'

    if (!user && !inAuthGroup) {
      router.replace('/(auth)/login')
    } else if (user && inAuthGroup) {
      router.replace('/(tabs)')
    }
  }, [user, isLoading, segments])

  // Sync access token to secure storage whenever user changes
  useEffect(() => {
    if (!user) { tokenStorage.remove(); return }
    getCredentials().then(creds => {
      if (creds?.accessToken) tokenStorage.set(creds.accessToken)
    }).catch(() => {})
  }, [user])

  return (
    <Stack screenOptions={{ headerShown: false }}>
      <Stack.Screen name="(auth)" />
      <Stack.Screen name="(tabs)" />
    </Stack>
  )
}

export default function RootLayout() {
  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <Auth0Provider domain={AUTH0_DOMAIN} clientId={AUTH0_CLIENT_ID}>
        <AuthGate />
      </Auth0Provider>
    </GestureHandlerRootView>
  )
}
