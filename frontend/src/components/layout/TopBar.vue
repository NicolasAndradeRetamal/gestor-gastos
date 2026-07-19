<template>
  <header class="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-line bg-surface-raised px-4 md:hidden">
    <h1 class="text-lg font-bold text-ink">{{ title }}</h1>
    <div ref="menuRef" class="relative">
      <button
        type="button"
        class="flex size-9 items-center justify-center rounded-full bg-primary-soft text-sm font-semibold text-primary"
        aria-haspopup="true"
        :aria-expanded="menuOpen"
        aria-label="Abrir menú de usuario"
        @click="menuOpen = !menuOpen"
      >
        {{ initials }}
      </button>
      <div
        v-if="menuOpen"
        class="absolute right-0 mt-2 w-48 rounded-md border border-line bg-surface-raised p-1 shadow-raised"
        role="menu"
      >
        <p class="truncate px-3 py-2 text-sm font-medium text-ink">{{ authStore.user?.displayName }}</p>
        <button
          type="button"
          role="menuitem"
          class="flex h-11 w-full items-center gap-2 rounded-md px-3 text-sm font-medium text-ink-muted hover:bg-surface-sunken"
          @click="onLogout"
        >
          <AppIcon name="logout" class="size-5" />
          Cerrar sesión
        </button>
        <div @click="menuOpen = false">
          <ThemeToggle variant="menu-item" />
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import AppIcon from '@/components/base/AppIcon.vue'
import ThemeToggle from '@/components/base/ThemeToggle.vue'
import { getInitials } from '@/composables/useInitials'
import { useClickOutside } from '@/composables/useClickOutside'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const title = computed(() => route.meta.title ?? 'Gestor de Gastos')
const initials = computed(() => getInitials(authStore.user?.displayName ?? '?'))

const menuOpen = ref(false)
const menuRef = ref<HTMLElement | null>(null)
useClickOutside(menuRef, () => (menuOpen.value = false))

function onLogout(): void {
  authStore.logout()
  router.push({ name: 'login' })
}
</script>
