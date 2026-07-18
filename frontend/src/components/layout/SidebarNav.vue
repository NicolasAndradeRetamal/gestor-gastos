<template>
  <aside class="sticky top-0 hidden h-dvh w-64 shrink-0 flex-col border-r border-line bg-surface-raised md:flex">
    <div class="flex h-16 items-center gap-2 border-b border-line px-6">
      <AppIcon name="wallet" class="size-6 text-primary" />
      <span class="text-lg font-bold text-ink">Gestor de Gastos</span>
    </div>
    <nav class="mt-3 flex flex-1 flex-col gap-1" aria-label="Navegación principal">
      <RouterLink
        v-for="item in NAV_ITEMS"
        :key="item.name"
        :to="{ name: item.name }"
        custom
        v-slot="{ href, navigate, isExactActive }"
      >
        <a
          :href="href"
          class="relative mx-3 flex h-11 items-center gap-3 rounded-md px-3 text-sm font-medium transition-colors"
          :class="
            isExactActive
              ? 'bg-primary-soft font-semibold text-primary'
              : 'text-ink-muted hover:bg-surface-sunken'
          "
          :aria-current="isExactActive ? 'page' : undefined"
          @click="navigate"
        >
          <span
            v-if="isExactActive"
            class="absolute -left-3 top-1/2 h-6 w-[3px] -translate-y-1/2 rounded-full bg-primary"
            aria-hidden="true"
          />
          <AppIcon :name="item.icon" class="size-5" />
          {{ item.label }}
        </a>
      </RouterLink>
    </nav>
    <div class="mt-auto flex items-center gap-3 border-t border-line p-3">
      <span
        class="flex size-9 shrink-0 items-center justify-center rounded-full bg-primary-soft text-sm font-semibold text-primary"
        aria-hidden="true"
      >
        {{ initials }}
      </span>
      <span class="truncate text-sm font-medium text-ink">{{ authStore.user?.displayName }}</span>
      <BaseButton
        variant="ghost"
        icon-only
        icon="logout"
        class="ml-auto"
        aria-label="Cerrar sesión"
        @click="onLogout"
      >
        Cerrar sesión
      </BaseButton>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'

import AppIcon from '@/components/base/AppIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { NAV_ITEMS } from '@/components/layout/navItems'
import { getInitials } from '@/composables/useInitials'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const router = useRouter()

const initials = computed(() => getInitials(authStore.user?.displayName ?? '?'))

function onLogout(): void {
  authStore.logout()
  router.push({ name: 'login' })
}
</script>
