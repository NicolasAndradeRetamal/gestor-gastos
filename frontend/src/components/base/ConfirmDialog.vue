<template>
  <Modal :open="open" :title="title" :close-on-overlay="false" :busy="busy" @close="emit('close')">
    <p class="text-sm text-ink-muted">{{ message }}</p>
    <div v-if="blocked" class="flex items-start gap-2 rounded-lg border border-warning/30 bg-warning-soft p-3">
      <AppIcon name="alert-triangle" class="size-5 shrink-0 text-warning" />
      <p class="text-sm text-ink">{{ blockedMessage }}</p>
    </div>
    <template #footer>
      <BaseButton variant="secondary" :disabled="busy" @click="emit('close')">Cancelar</BaseButton>
      <BaseButton variant="danger" :loading="busy" :disabled="blocked" @click="emit('confirm')">
        {{ confirmLabel ?? 'Eliminar' }}
      </BaseButton>
    </template>
  </Modal>
</template>

<script setup lang="ts">
import AppIcon from '@/components/base/AppIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import Modal from '@/components/base/Modal.vue'

defineProps<{
  open: boolean
  title: string
  message: string
  confirmLabel?: string
  busy?: boolean
  blocked?: boolean
  blockedMessage?: string
}>()

const emit = defineEmits<{
  close: []
  confirm: []
}>()
</script>
