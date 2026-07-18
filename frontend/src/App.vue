<template>
  <AppShell v-if="route.meta.requiresAuth">
    <RouterView />
  </AppShell>
  <RouterView v-else />
  <ToastContainer />
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'

import ToastContainer from '@/components/base/ToastContainer.vue'
import AppShell from '@/components/layout/AppShell.vue'

const route = useRoute()
</script>
