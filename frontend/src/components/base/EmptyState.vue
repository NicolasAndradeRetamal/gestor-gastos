<template>
  <div class="flex flex-col items-center gap-3 rounded-xl border border-dashed border-line px-6 py-12 text-center">
    <AppIcon :name="icon ?? 'search-off'" class="size-10 text-ink-muted" />
    <p class="text-sm font-semibold text-ink">{{ title }}</p>
    <p class="text-sm text-ink-muted">{{ description }}</p>
    <BaseButton v-if="actionLabel" variant="primary" icon="plus" @click="emit('action')">
      {{ actionLabel }}
    </BaseButton>
  </div>
</template>

<script setup lang="ts">
import AppIcon, { type IconName } from '@/components/base/AppIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'

defineProps<{
  title: string
  description: string
  actionLabel?: string
  icon?: IconName
}>()

const emit = defineEmits<{
  action: []
}>()
</script>
