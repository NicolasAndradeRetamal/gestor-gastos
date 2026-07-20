<template>
  <header class="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-line bg-surface-raised px-4 md:hidden">
    <h1 class="text-lg font-bold text-ink">{{ title }}</h1>
    <UserMenu :display-name="authStore.user?.displayName ?? '?'" :email="authStore.user?.email" />
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

import UserMenu from '@/components/layout/UserMenu.vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const route = useRoute()

const title = computed(() => route.meta.title ?? 'Gestor de Gastos')
</script>
