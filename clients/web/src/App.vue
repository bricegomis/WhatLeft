<template>
  <v-app>
    <RouterView />
  </v-app>
</template>

<script setup lang="ts">
import { watch } from 'vue'
import { useAuth0 } from '@auth0/auth0-vue'
import { setAuthToken } from './services/tasksApi'
import { setRecurringAuthToken } from './services/recurringApi'

const { isAuthenticated, getAccessTokenSilently } = useAuth0()

watch(isAuthenticated, async (authenticated) => {
  if (authenticated) {
    const token = await getAccessTokenSilently()
    setAuthToken(token)
    setRecurringAuthToken(token)
  } else {
    setAuthToken(null)
    setRecurringAuthToken(null)
  }
}, { immediate: true })
</script>
