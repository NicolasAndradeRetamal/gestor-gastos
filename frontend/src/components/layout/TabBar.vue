<template>
  <nav
    class="fixed inset-x-0 bottom-0 z-10 flex border-t border-line bg-surface-raised pb-[env(safe-area-inset-bottom)] md:hidden"
    aria-label="Navegación principal"
  >
    <RouterLink
      v-for="item in NAV_ITEMS"
      :key="item.name"
      :to="{ name: item.name }"
      custom
      v-slot="{ href, navigate, isExactActive }"
    >
      <a
        :href="href"
        class="flex h-16 flex-1 flex-col items-center justify-center gap-0.5 text-xs font-medium"
        :class="isExactActive ? 'text-primary' : 'text-ink-muted'"
        :aria-current="isExactActive ? 'page' : undefined"
        @click="navigate"
      >
        <AppIcon :name="item.icon" class="size-5" />
        {{ item.label }}
      </a>
    </RouterLink>
  </nav>
</template>

<script setup lang="ts">
import AppIcon from '@/components/base/AppIcon.vue'
import { NAV_ITEMS } from '@/components/layout/navItems'
</script>
