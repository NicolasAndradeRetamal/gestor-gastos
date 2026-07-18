<template>
  <div class="flex min-h-dvh flex-col items-center justify-center gap-4 p-4 text-center">
    <AppIcon name="search-off" class="size-10 text-ink-muted" />
    <h1 class="text-2xl font-bold text-ink">Página no encontrada</h1>
    <p class="text-sm text-ink-muted">La página que buscás no existe o fue movida.</p>
    <BaseButton @click="router.push('/')">Volver al inicio</BaseButton>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'

import AppIcon from '@/components/base/AppIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'

const router = useRouter()
</script>
