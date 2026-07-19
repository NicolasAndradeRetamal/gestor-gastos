<template>
  <BaseButton
    v-if="variant === 'icon'"
    variant="ghost-muted"
    icon-only
    :icon="icon"
    :aria-label="label"
    @click="toggle"
  >
    {{ label }}
  </BaseButton>
  <button
    v-else
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
import BaseButton from '@/components/base/BaseButton.vue'
import { useTheme } from '@/composables/useTheme'

withDefaults(
  defineProps<{
    // 'icon': icon-only button for the desktop sidebar. 'menu-item': labelled row for the mobile user menu.
    variant?: 'icon' | 'menu-item'
  }>(),
  { variant: 'icon' },
)

const { theme, toggle } = useTheme()

// Shows the active theme (not the destination), the standard convention for this control.
const icon = computed<IconName>(() => (theme.value === 'dark' ? 'moon' : 'sun'))

const label = computed(() => (theme.value === 'dark' ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro'))
const menuLabel = computed(() => (theme.value === 'dark' ? 'Modo claro' : 'Modo oscuro'))
</script>
