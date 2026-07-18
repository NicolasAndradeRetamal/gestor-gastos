<template>
  <div class="flex min-h-dvh">
    <SidebarNav />
    <div class="flex min-w-0 flex-1 flex-col">
      <TopBar />
      <main class="mx-auto w-full max-w-6xl flex-1 px-4 py-6 pb-20 sm:px-6 md:pb-6 lg:px-8">
        <slot />
      </main>
      <TabBar />
    </div>
  </div>
</template>

<script setup lang="ts">
import SidebarNav from '@/components/layout/SidebarNav.vue'
import TabBar from '@/components/layout/TabBar.vue'
import TopBar from '@/components/layout/TopBar.vue'
</script>
