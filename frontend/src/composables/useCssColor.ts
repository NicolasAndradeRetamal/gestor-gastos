import { onBeforeUnmount, onMounted, ref, type Ref } from 'vue'

// Resolves a `--color-*` custom property (defined with `light-dark()`) to its
// current computed value, since <canvas> fillStyle can't resolve var()/light-dark()
// itself. Re-reads when the OS color-scheme preference changes.
export function useCssColor(variableName: string): Ref<string> {
  const value = ref('')

  function read(): void {
    value.value = getComputedStyle(document.documentElement).getPropertyValue(variableName).trim()
  }

  const media = window.matchMedia('(prefers-color-scheme: dark)')

  onMounted(() => {
    read()
    media.addEventListener('change', read)
  })
  onBeforeUnmount(() => media.removeEventListener('change', read))

  return value
}
