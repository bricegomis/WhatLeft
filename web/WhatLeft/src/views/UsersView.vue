<template>
  <AdminLayout>
    <!-- Page Header -->
    <v-row class="mb-6">
      <v-col cols="12">
        <div>
          <h1 class="text-h4 font-weight-bold mb-2">Utilisateurs</h1>
          <p class="text-body-1 text-medium-emphasis mb-0">
            Gérer les comptes et les rôles des membres.
          </p>
        </div>
      </v-col>
    </v-row>

    <!-- Users Table -->
    <v-card>
      <v-card-title class="text-h6">Liste des utilisateurs</v-card-title>
      <v-data-table
        :headers="tableHeaders"
        :items="users"
        item-key="id"
      >
        <template #item.role="{ item }">
          <v-chip
            :color="getRoleColor(item.role)"
            size="small"
            variant="flat"
          >
            {{ item.role }}
          </v-chip>
        </template>
      </v-data-table>
    </v-card>
  </AdminLayout>
</template>

<script setup lang="ts">
import AdminLayout from '../layouts/AdminLayout.vue'

interface User {
  id: string
  name: string
  email: string
  role: string
}

const tableHeaders = [
  { title: 'Nom', key: 'name', width: '30%' },
  { title: 'Email', key: 'email', width: '40%' },
  { title: 'Rôle', key: 'role', width: '30%' }
]

const users: User[] = [
  {
    id: '1',
    name: 'Alexandre Dupont',
    email: 'alexandre@whatleft.fr',
    role: 'Admin'
  },
  {
    id: '2',
    name: 'Emma Rousseau',
    email: 'emma@whatleft.fr',
    role: 'Éditeur'
  },
  {
    id: '3',
    name: 'Marc Chen',
    email: 'marc@whatleft.fr',
    role: 'Lecteur'
  }
]

const getRoleColor = (role: string) => {
  const colors: Record<string, string> = {
    'Admin': 'primary',
    'Éditeur': 'info',
    'Lecteur': 'secondary'
  }
  return colors[role] || 'default'
}
</script>
