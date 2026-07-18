<template>
  <div
    role="alert"
    class="flex flex-col items-center gap-3 rounded-xl border border-danger/30 bg-danger-soft px-6 py-12 text-center"
  >
    <AppIcon name="alert-circle" class="size-10 text-danger" />
    <p class="text-sm font-semibold text-ink">No se pudo cargar la información</p>
    <p class="text-sm text-ink-muted">{{ message ?? 'Comprueba tu conexión e inténtalo de nuevo.' }}</p>
    <BaseButton variant="secondary" @click="emit('retry')">Reintentar</BaseButton>
  </div>
</template>

<script setup lang="ts">
import AppIcon from '@/components/base/AppIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'

defineProps<{
  message?: string
}>()

const emit = defineEmits<{
  retry: []
}>()
</script>
