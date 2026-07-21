<template>
  <div ref="rootRef" class="relative" :class="variant === 'sidebar' ? 'mt-auto shrink-0' : undefined">
    <button
      v-if="variant === 'sidebar'"
      type="button"
      class="flex w-full items-center gap-3 rounded-none border-t border-line p-3 text-left transition-colors hover:bg-surface-sunken"
      aria-haspopup="menu"
      :aria-expanded="isOpen"
      :aria-controls="menuId"
      @click="toggleMenu"
    >
      <span
        class="flex size-9 shrink-0 items-center justify-center rounded-full bg-primary-soft text-sm font-semibold text-primary"
        aria-hidden="true"
      >
        {{ initials }}
      </span>
      <span class="flex-1 truncate text-sm font-medium text-ink">{{ displayName }}</span>
      <AppIcon name="chevron-down" class="size-5 shrink-0 text-ink-muted" />
    </button>
    <button
      v-else
      type="button"
      class="flex size-9 items-center justify-center rounded-full bg-primary-soft text-sm font-semibold text-primary"
      aria-haspopup="menu"
      :aria-expanded="isOpen"
      :aria-controls="menuId"
      aria-label="Abrir menú de usuario"
      @click="toggleMenu"
    >
      {{ initials }}
    </button>

    <Transition
      enter-active-class="transition duration-150 ease-out"
      :enter-from-class="menuFromClass"
      leave-active-class="transition duration-150 ease-in"
      :leave-to-class="menuFromClass"
    >
      <div
        v-if="isOpen"
        :id="menuId"
        ref="menuRef"
        role="menu"
        :aria-label="`Menú de ${displayName}`"
        class="absolute z-20 w-56 rounded-md border border-line bg-surface-raised p-1 shadow-raised"
        :class="variant === 'sidebar' ? 'bottom-full left-3 mb-2 origin-bottom' : 'right-0 top-full mt-2 origin-top'"
        @keydown.esc="closeMenu"
        @keydown.tab="onTab"
      >
      <div class="px-3 py-2" role="presentation">
        <p class="truncate text-sm font-medium text-ink">{{ displayName }}</p>
        <p v-if="email" class="truncate text-xs text-ink-muted">{{ email }}</p>
      </div>
      <div @click="closeMenu">
        <ThemeToggle />
      </div>
      <div class="my-1 border-t border-line" role="separator" />
      <button
        type="button"
        role="menuitem"
        class="flex h-11 w-full items-center gap-2 rounded-md px-3 text-sm font-medium text-ink-muted hover:bg-surface-sunken"
        @click="onNavigate('security')"
      >
        <AppIcon name="shield" class="size-5" />
        Seguridad
      </button>
      <button
        type="button"
        role="menuitem"
        class="flex h-11 w-full items-center gap-2 rounded-md px-3 text-sm font-medium text-ink-muted hover:bg-surface-sunken"
        @click="onLogout"
      >
        <AppIcon name="logout" class="size-5" />
        Cerrar sesión
      </button>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, useId } from 'vue'
import { useRouter } from 'vue-router'

import AppIcon from '@/components/base/AppIcon.vue'
import ThemeToggle from '@/components/base/ThemeToggle.vue'
import { trapTabKey, useFocusTrap } from '@/composables/useFocusTrap'
import { getInitials } from '@/composables/useInitials'
import { useClickOutside } from '@/composables/useClickOutside'
import { useAuthStore } from '@/stores/auth'

const props = withDefaults(
  defineProps<{
    displayName: string
    email?: string
    // 'sidebar': full-width footer trigger, menu opens upward. 'topbar': avatar-only trigger, menu opens downward.
    variant?: 'sidebar' | 'topbar'
  }>(),
  { variant: 'topbar' },
)

const authStore = useAuthStore()
const router = useRouter()

const menuId = useId()
const rootRef = ref<HTMLElement | null>(null)
const { panelRef: menuRef, activate, deactivate } = useFocusTrap()
const isOpen = ref(false)

const initials = computed(() => getInitials(props.displayName))

// The popover animates from the edge it's anchored to (up for sidebar, down for topbar).
const menuFromClass = computed(() =>
  props.variant === 'sidebar'
    ? 'opacity-0 translate-y-1 scale-95 motion-reduce:transform-none'
    : 'opacity-0 -translate-y-1 scale-95 motion-reduce:transform-none',
)

useClickOutside(rootRef, () => {
  if (isOpen.value) closeMenu()
})

async function openMenu(): Promise<void> {
  isOpen.value = true
  activate()
  await nextTick()
  menuRef.value?.querySelector<HTMLElement>('[role="menuitem"], button')?.focus()
}

function closeMenu(): void {
  if (!isOpen.value) return
  isOpen.value = false
  deactivate()
}

function toggleMenu(): void {
  if (isOpen.value) {
    closeMenu()
  } else {
    openMenu()
  }
}

function onTab(event: KeyboardEvent): void {
  trapTabKey(event, menuRef.value)
}

function onNavigate(name: string): void {
  closeMenu()
  router.push({ name })
}

function onLogout(): void {
  closeMenu()
  void authStore.logout()
  router.push({ name: 'login' })
}
</script>
