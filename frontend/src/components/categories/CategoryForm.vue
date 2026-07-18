<template>
  <Modal :open="open" :title="isEditing ? 'Editar categoría' : 'Crear categoría'" :busy="busy" @close="emit('close')">
    <p v-if="bannerMessage" role="alert" class="rounded-lg border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
      {{ bannerMessage }}
    </p>
    <form id="category-form" class="space-y-4" novalidate @submit.prevent="onSubmit">
      <BaseInput v-model="name" label="Nombre" required maxlength="60" :error="fieldError('name')" />
      <ColorPicker v-model="color" label="Color" />
    </form>
    <template #footer>
      <BaseButton variant="secondary" :disabled="busy" @click="emit('close')">Cancelar</BaseButton>
      <BaseButton type="submit" form="category-form" :loading="busy" loading-label="Guardando…">
        {{ isEditing ? 'Guardar cambios' : 'Guardar categoría' }}
      </BaseButton>
    </template>
  </Modal>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'

import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import ColorPicker from '@/components/base/ColorPicker.vue'
import Modal from '@/components/base/Modal.vue'
import { CATEGORY_COLORS } from '@/lib/categoryColors'
import type { Category, CategoryRequest, ProblemDetails } from '@/types'

const props = defineProps<{
  open: boolean
  category: Category | null
  busy: boolean
  error: ProblemDetails | null
}>()

const emit = defineEmits<{
  close: []
  submit: [payload: CategoryRequest]
}>()

const name = ref('')
const color = ref<string>(CATEGORY_COLORS[0])

const isEditing = computed(() => Boolean(props.category))

function resetForm(): void {
  name.value = props.category?.name ?? ''
  color.value = props.category?.color ?? CATEGORY_COLORS[0]
}

watch(
  () => props.open,
  (isOpen) => {
    if (isOpen) resetForm()
  },
)

function fieldError(field: string): string | undefined {
  if (field === 'name' && props.error?.status === 409) {
    return 'Ya tienes una categoría con ese nombre.'
  }
  return props.error?.errors?.[field]?.[0]
}

const bannerMessage = computed(() => {
  const error = props.error
  if (!error || error.errors) return null
  if (error.status === 409) return null
  return error.detail ?? 'No se pudo guardar la categoría.'
})

function onSubmit(): void {
  emit('submit', { name: name.value, color: color.value })
}
</script>
