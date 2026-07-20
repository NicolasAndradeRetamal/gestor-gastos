<template>
  <Teleport to="body">
    <div v-if="open" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="fixed inset-0 bg-zinc-950/50 backdrop-blur-[1px]" @click="onOverlayClick" />
      <div
        ref="panelRef"
        role="dialog"
        aria-modal="true"
        :aria-labelledby="titleId"
        class="relative w-full max-w-md rounded-xl bg-surface-raised p-6 shadow-modal"
        @keydown.esc="onEscape"
        @keydown.tab="onTab"
      >
        <div class="mb-4 flex items-center justify-between">
          <h2 :id="titleId" tabindex="-1" class="text-lg font-semibold text-ink outline-none">{{ title }}</h2>
          <BaseButton variant="ghost" icon-only icon="close" aria-label="Cerrar" :disabled="busy" @click="close">
            Cerrar
          </BaseButton>
        </div>
        <div ref="bodyRef" class="space-y-4">
          <slot />
        </div>
        <div v-if="$slots.footer" class="mt-6 flex items-center justify-end gap-3">
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { nextTick, ref, useId, watch } from 'vue'

import BaseButton from '@/components/base/BaseButton.vue'
import { trapTabKey, useFocusTrap } from '@/composables/useFocusTrap'

const props = defineProps<{
  open: boolean
  title: string
  closeOnOverlay?: boolean
  busy?: boolean
}>()

const emit = defineEmits<{
  close: []
}>()

const titleId = useId()
const bodyRef = ref<HTMLElement | null>(null)
const { panelRef, activate, deactivate } = useFocusTrap()

function close(): void {
  if (props.busy) return
  emit('close')
}

function onOverlayClick(): void {
  if (props.closeOnOverlay === false) return
  close()
}

function onEscape(): void {
  close()
}

function onTab(event: KeyboardEvent): void {
  trapTabKey(event, panelRef.value)
}

watch(
  () => props.open,
  async (isOpen) => {
    if (isOpen) {
      activate()
      await nextTick()
      const firstField = bodyRef.value?.querySelector<HTMLElement>('input, textarea, select, button')
      if (firstField) {
        firstField.focus()
      } else {
        document.getElementById(titleId)?.focus()
      }
    } else {
      deactivate()
    }
  },
)
</script>
