import { onBeforeUnmount, onMounted, type Ref } from 'vue'

export function useClickOutside(target: Ref<HTMLElement | null>, onOutside: () => void): void {
  function handleClick(event: MouseEvent): void {
    const el = target.value
    if (el && !el.contains(event.target as Node)) {
      onOutside()
    }
  }

  onMounted(() => document.addEventListener('click', handleClick))
  onBeforeUnmount(() => document.removeEventListener('click', handleClick))
}
