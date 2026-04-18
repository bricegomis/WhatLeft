<template>
  <div class="d-flex align-center justify-center" style="min-height: 100vh; background: linear-gradient(135deg, #eef2ff 0%, #f8fafc 100%);">
    <v-card width="100%" max-width="460">
      <v-card-title class="text-h4 font-weight-bold text-center">Connexion</v-card-title>
      
      <v-card-text>
        <v-text-field
          label="Adresse email"
          placeholder="bonjour@exemple.com"
          type="email"
          prepend-inner-icon="mdi-email"
          class="mb-4"
        />

        <v-text-field
          label="Mot de passe"
          placeholder="••••••••"
          type="password"
          prepend-inner-icon="mdi-lock"
        />
      </v-card-text>

      <v-card-actions>
        <v-spacer />
        <v-btn
          color="primary"
          size="large"
          @click="login"
        >
          Se connecter
        </v-btn>
      </v-card-actions>
    </v-card>
  </div>
</template>

<script setup lang="ts">
const login = () => {
  console.log('Connexion...')
}
</script>
