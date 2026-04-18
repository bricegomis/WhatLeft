<template>
  <AdminLayout>
    <section class="page-header">
      <div>
        <h1>Utilisateurs</h1>
        <p>Gérer les comptes et les rôles des membres.</p>
      </div>
    </section>

    <section class="panel">
      <h2>Liste des utilisateurs</h2>
      <table class="table">
        <thead>
          <tr>
            <th>Nom</th>
            <th>Email</th>
            <th>Rôle</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>Alexandre Dupont</td>
            <td>alexandre@whatleft.fr</td>
            <td>Admin</td>
          </tr>
          <tr>
            <td>Emma Rousseau</td>
            <td>emma@whatleft.fr</td>
            <td>Éditeur</td>
          </tr>
          <tr>
            <td>Marc Chen</td>
            <td>marc@whatleft.fr</td>
            <td>Lecteur</td>
          </tr>
        </tbody>
      </table>
    </section>
  </AdminLayout>
</template>

<script setup lang="ts">
import AdminLayout from '../layouts/AdminLayout.vue'
</script>
