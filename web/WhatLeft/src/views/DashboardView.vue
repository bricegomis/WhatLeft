<template>
  <AdminLayout>
    <section class="page-header">
      <div>
        <h1>Tableau de bord</h1>
        <p>Vue d'ensemble de l'activité et des performances.</p>
      </div>
    </section>

    <div class="grid">
      <article class="card">
        <h2>Revenu</h2>
        <strong>24 580 €</strong>
        <p>+12 % par rapport à la semaine dernière.</p>
      </article>
      <article class="card">
        <h2>Nouveaux utilisateurs</h2>
        <strong>1 240</strong>
        <p>Analyse rapide des inscriptions.</p>
      </article>
      <article class="card">
        <h2>Tickets</h2>
        <strong>18 ouverts</strong>
        <p>Suivi des demandes en cours.</p>
      </article>
    </div>

    <section class="panel">
      <h2>Activité récente</h2>
      <table class="table">
        <thead>
          <tr>
            <th>Action</th>
            <th>Utilisateur</th>
            <th>Statut</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>Mise à jour du financement</td>
            <td>Claire Martin</td>
            <td>Complété</td>
          </tr>
          <tr>
            <td>Ajout d'un nouveau projet</td>
            <td>Romain Lacroix</td>
            <td>En attente</td>
          </tr>
          <tr>
            <td>Connexion API</td>
            <td>Laura Nguyen</td>
            <td>Erreur</td>
          </tr>
        </tbody>
      </table>
    </section>
  </AdminLayout>
</template>

<script setup lang="ts">
import AdminLayout from '../layouts/AdminLayout.vue'
</script>
