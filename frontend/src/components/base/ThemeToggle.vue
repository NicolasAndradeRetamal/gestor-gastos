<template>
  <button
    type="button"
    role="menuitem"
    class="flex h-11 w-full items-center gap-2 rounded-md px-3 text-sm font-medium text-ink-muted hover:bg-surface-sunken"
    @click="toggle"
  >
    <AppIcon :name="icon" class="size-5" />
    {{ menuLabel }}
  </button>
</template>

<script setup lang="ts">
import { computed } from 'vue'

import AppIcon, { type IconName } from '@/components/base/AppIcon.vue'
import { useTheme } from '@/composables/useTheme'

const { theme, toggle } = useTheme()

// Shows the active theme (not the destination), the standard convention for this control.
const icon = computed<IconName>(() => (theme.value === 'dark' ? 'moon' : 'sun'))

const menuLabel = computed(() => (theme.value === 'dark' ? 'Modo claro' : 'Modo oscuro'))
</script>
