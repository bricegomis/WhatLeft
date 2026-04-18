<template>
  <AdminLayout>
    <section class="page-header">
      <div>
        <h1>Paramètres</h1>
        <p>Configurer le tableau de bord et les préférences système.</p>
      </div>
    </section>

    <section class="panel">
      <h2>Préférences générales</h2>
      <div class="form-group">
        <label for="site-name">Nom du site</label>
        <input id="site-name" type="text" value="WhatLeft Admin" />
      </div>
      <div class="form-group">
        <label for="timezone">Fuseau horaire</label>
        <select id="timezone">
          <option>Europe/Paris</option>
          <option>Europe/London</option>
          <option>UTC</option>
        </select>
      </div>
      <button class="primary">Sauvegarder</button>
    </section>
  </AdminLayout>
</template>

<script setup lang="ts">
import AdminLayout from '../layouts/AdminLayout.vue'
</script>
