<template>
  <div class="d-flex align-center justify-center" style="min-height: 100vh; background: linear-gradient(135deg, #eef2ff 0%, #f8fafc 100%);">
    <v-card width="100%" max-width="460" class="pa-4">
      <v-card-title class="text-h4 font-weight-bold text-center mb-2">WhatLeft</v-card-title>
      <v-card-subtitle class="text-center mb-6">Connectez-vous pour continuer</v-card-subtitle>

      <v-card-text class="d-flex flex-column gap-3">
        <v-btn
          size="large"
          variant="outlined"
          block
          prepend-icon="mdi-google"
          @click="loginWithGoogle"
        >
          Continuer avec Google
        </v-btn>

        <v-btn
          size="large"
          variant="outlined"
          block
          prepend-icon="mdi-microsoft"
          @click="loginWithMicrosoft"
        >
          Continuer avec Microsoft
        </v-btn>

        <v-btn
          size="large"
          variant="outlined"
          block
          prepend-icon="mdi-github"
          @click="loginWithGithub"
        >
          Continuer avec GitHub
        </v-btn>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { useAuth0 } from '@auth0/auth0-vue'

const { loginWithRedirect } = useAuth0()

const loginWithGoogle = () =>
  loginWithRedirect({ authorizationParams: { connection: 'google-oauth2' } })

const loginWithMicrosoft = () =>
  loginWithRedirect({ authorizationParams: { connection: 'windowslive' } })

const loginWithGithub = () =>
  loginWithRedirect({ authorizationParams: { connection: 'github' } })
</script>
