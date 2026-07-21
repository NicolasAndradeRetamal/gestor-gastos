<template>
  <div ref="root" class="relative">
    <BaseButton
      variant="secondary"
      icon="chevron-down"
      class="w-full sm:w-auto"
      aria-haspopup="menu"
      :aria-expanded="open"
      @click="toggle"
    >
      Más
    </BaseButton>
    <Transition
      enter-active-class="transition duration-150 ease-out"
      enter-from-class="opacity-0 -translate-y-1 scale-95 motion-reduce:transform-none"
      leave-active-class="transition duration-150 ease-in"
      leave-to-class="opacity-0 -translate-y-1 scale-95 motion-reduce:transform-none"
    >
      <div
        v-if="open"
        role="menu"
        class="absolute right-0 top-full z-20 mt-2 w-56 origin-top rounded-md border border-line bg-surface-raised p-1 shadow-raised"
        @keydown.esc="close"
      >
        <button type="button" role="menuitem" :class="rowClass" @click="select('recurring')">
          <AppIcon name="expenses" class="size-5" />
          Gastos recurrentes
        </button>
        <button type="button" role="menuitem" :class="rowClass" @click="select('import')">
          <AppIcon name="upload" class="size-5" />
          Importar CSV
        </button>
        <button type="button" role="menuitem" :class="rowClass" @click="select('export')">
          <AppIcon name="download" class="size-5" />
          Exportar CSV
        </button>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, useTemplateRef } from 'vue'

import AppIcon from '@/components/base/AppIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useClickOutside } from '@/composables/useClickOutside'

const emit = defineEmits<{
  select: [action: 'recurring' | 'import' | 'export']
}>()

const rowClass =
  'flex h-11 w-full items-center gap-2 rounded-md px-3 text-sm font-medium text-ink-muted hover:bg-surface-sunken'

const root = useTemplateRef<HTMLElement>('root')
const open = ref(false)

useClickOutside(root, () => {
  open.value = false
})

function toggle(): void {
  open.value = !open.value
}

function close(): void {
  open.value = false
}

function select(action: 'recurring' | 'import' | 'export'): void {
  open.value = false
  emit('select', action)
}
</script>
