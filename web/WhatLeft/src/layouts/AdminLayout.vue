<template>
  <div class="app-shell">
    <aside class="sidebar">
      <div class="brand">WhatLeft</div>
      <nav>
        <RouterLink to="/" class="nav-link" active-class="active" exact>Tableau de bord</RouterLink>
        <RouterLink to="/users" class="nav-link" active-class="active">Utilisateurs</RouterLink>
        <RouterLink to="/settings" class="nav-link" active-class="active">Paramètres</RouterLink>
      </nav>
      <p class="section-title">Support</p>
      <RouterLink to="/login" class="nav-link" active-class="active">Se connecter</RouterLink>
    </aside>

    <div class="content-area">
      <header class="topbar">
        <div>
          <input type="text" placeholder="Recherche..." aria-label="Recherche" />
        </div>
        <div class="user-card">
          <div class="user-avatar">A</div>
          <div>
            <div>Administrateur</div>
            <small>whatleft@example.com</small>
          </div>
        </div>
      </header>
      <main class="main-content">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts"></script>
